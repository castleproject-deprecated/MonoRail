namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
	using System.ServiceModel.Syndication;
	using System.Text;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public partial class SegmentProcessorTestCase
	{
		private StringBuilder _body;
		private ResponseParameters _response;
		private IQueryable<Catalog1> _catalog1Set;
		private IQueryable<Product1> _product1Set;
		private IQueryable<Supplier1> _supplier1Set;
		private StubModel _model;
		private List<Tuple<ResourceType, object>> _accessSingle;
		private List<Tuple<ResourceType, IEnumerable>> _accessMany;
		private List<Tuple<ResourceType, object>> _created;
		private List<Tuple<ResourceType, object>> _updated;
		private List<Tuple<ResourceType, object>> _removed;

		private StubModel _modelWithMinimalContainer;

		public class ProcessorAssertions
		{
			private static SegmentProcessorTestCase state;
			public CallbackAsserts Callbacks = new CallbackAsserts();

			public ProcessorAssertions(SegmentProcessorTestCase state)
			{
				ProcessorAssertions.state = state;
			}

			public class CallbackAsserts
			{
				public void SingleWasCalled(int howManyTimes)
				{
					state._accessSingle.Count.Should().Be(howManyTimes);
				}
				public void ManyWasCalled(int howManyTimes)
				{
					state._accessMany.Count.Should().Be(howManyTimes);
				}
			}

			public void Entry(SyndicationItem item, string Id)
			{
				item.Should().NotBeNull();
				item.BaseUri.OriginalString.Should().Be("http://localhost/base/");
				item.Id.Should().BeEquivalentTo(Id);
			}

			public void EntryLink(SyndicationItem item, string Title, string Rel = null, string Href = null, string Media = null)
			{
				var link = item.Links.Where(l => l.Title == Title).SingleOrDefault();
				link.Should().NotBeNull("Could not find link with title " + Title);
				link.BaseUri.OriginalString.Should().Be("http://localhost/base/");
				link.Title.Should().Be(Title);
				if (Rel != null)
					link.RelationshipType.Should().Be(Rel);
				if (Media != null)
					link.MediaType.Should().Be(Media);
				if (Href != null)
					link.Uri.OriginalString.Should().BeEquivalentTo(Href);
			}

			public void Feed(SyndicationFeed item, string id)
			{
				item.Should().NotBeNull();
				item.BaseUri.OriginalString.Should().Be("http://localhost/base/");
				item.Id.Should().BeEquivalentTo(id);
			}

			public void FeedLink(SyndicationFeed item, string title, string rel = null, string href = null, string media = null)
			{
				var link = item.Links.Where(l => l.Title == title).SingleOrDefault();
				link.Should().NotBeNull("Could not find link with title " + title);
				link.BaseUri.OriginalString.Should().Be("http://localhost/base/");
				link.Title.Should().Be(title);
				if (rel != null)
					link.RelationshipType.Should().Be(rel);
				if (media != null)
					link.MediaType.Should().Be(media);
				if (href != null)
					link.Uri.OriginalString.Should().BeEquivalentTo(href);
			}
		}

		public ProcessorAssertions Assertion;

		[SetUp]
		public void Init()
		{
			Assertion = new ProcessorAssertions(this);

			_accessSingle = new List<Tuple<ResourceType, object>>();
			_accessMany = new List<Tuple<ResourceType, IEnumerable>>();
			_created = new List<Tuple<ResourceType, object>>();
			_updated = new List<Tuple<ResourceType, object>>();
			_removed = new List<Tuple<ResourceType, object>>();

			_product1Set = new List<Product1>
			               	{
			               		new Product1() { Id = 1, Name = "Product1" },
								new Product1() { Id = 2, Name = "Product2" },
			               	}.AsQueryable();
			_catalog1Set = new List<Catalog1>
			               	{
			               		new Catalog1() { Id = 1, Name = "Cat1"}, 
								new Catalog1() { Id = 2, Name = "Cat2" }
			               	}.AsQueryable();
			
			_supplier1Set = new List<Supplier1>
							{
								new Supplier1() { Id = 1, Address = new Address1() { Street = "wilson ave", Zip = "vxxxx", Country = "canada"} },
								new Supplier1() { Id = 2, Address = new Address1() { Street = "kingsway ave", Zip = "zxxxx", Country = "canada"} },
							}.AsQueryable();

			_catalog1Set.ElementAt(0).Products = new List<Product1>(_product1Set);
			_catalog1Set.ElementAt(1).Products = new List<Product1>(_product1Set);
			_product1Set.ElementAt(0).Catalog = _catalog1Set.ElementAt(0);
			_product1Set.ElementAt(1).Catalog = _catalog1Set.ElementAt(1);

			_model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			_modelWithMinimalContainer = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
				});

			_body = new StringBuilder();
		}

		public void Process(string fullPath, SegmentOp operation, ODataModel model, 
							string contentType = "application/atom+xml", 
							string accept = "application/atom+xml", 
							Stream inputStream = null)
		{
			_body = new StringBuilder();
			var baseUri = new Uri("http://localhost/base/");

			var segments = SegmentParser.parse(fullPath, String.Empty, model, baseUri);
			_response = new ResponseParameters(null, Encoding.UTF8, new StringWriter(_body), 200);

			var callbacks = new ProcessorCallbacks(
					(rt, item) => 
					{ 
						_accessSingle.Add(new Tuple<ResourceType, object>(rt, item));
						return true; 
					},
					(rt, items) =>
					{
						_accessMany.Add(new Tuple<ResourceType, IEnumerable>(rt, items));
						return true;
					},
					(rt, item) =>
					{
						_created.Add(new Tuple<ResourceType, object>(rt, item));
						return true;
					},
					(rt, item) =>
					{
						_updated.Add(new Tuple<ResourceType, object>(rt, item));
						return true;
					},
					(rt, item) =>
					{
						_removed.Add(new Tuple<ResourceType, object>(rt, item));
						return true;
					}
				);

			SegmentProcessor.Process(operation, segments, 

				callbacks, 

				new RequestParameters(
					model, 
					model as IDataServiceMetadataProvider,
					new DataServiceMetadataProviderWrapper(model), 
					contentType, 
					Encoding.UTF8,
					inputStream,
					baseUri, 
					new [] { accept }
				),

				_response
			);
		}

		// -------------------------------------

		public class Address1
		{
			public string Street { get; set; }
			public string Zip { get; set; }
			public string Country { get; set; }
		}

		public class Supplier1
		{
			[Key]
			public int Id { get; set; }
			public Address1 Address { get; set; }
		}

		public class Catalog1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product1> Products { get; set; }
		}
		public class Product1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public Catalog1 Catalog { get; set; }
		}
	}
}
