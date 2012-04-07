namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
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

		[SetUp]
		public void Init()
		{
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

			_body = new StringBuilder();
		}

		public void Process(string fullPath, SegmentOp operation, ODataModel model, 
							string contentType = "application/atom+xml", string accept = "application/atom+xml")
		{
			_body = new StringBuilder();

			var segments = SegmentParser.parse(fullPath, String.Empty, model);
			_response = new ResponseParameters(null, Encoding.UTF8, new StringWriter(_body), 200);

			SegmentProcessor.Process(operation, segments, 
				
				new RequestParameters(
					model, 
					model as IDataServiceMetadataProvider,
					new DataServiceMetadataProviderWrapper(model), 
					contentType, 
					Encoding.UTF8, null,
					new Uri("http://localhost/base/svc"), 
					new [] { accept }
				),

				_response
			);
		}

		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void EntitySet_PropertySingle_View_Atom_Atom__Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/catalogs(1)/Id", SegmentOp.View, model);

			_response.contentType.Should().Be("application/atom+xml");
			_body.Should().Be("");
		}

		[Test]
		public void EntitySet_PropertySingle_View_Atom_Atom__Success_2()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			
			Process("/catalogs(1)/Id/", SegmentOp.View, model);
		}

//		[Test]
//		public void aaaaaaaaaa4()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)/Products", SegmentOp.View, model);
//
			// assert last segment is list of products
//		}
//
//		[Test]
//		public void aaaaaaaaaa4_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)/Products(1)", SegmentOp.View, model);
//
			// assert last segment is single product
//		}
//
//		[Test]
//		public void aaaaaaaaaa4__()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)/Products(1)/Name", SegmentOp.View, model);
//
			// assert last segment is product name
//		}
//
//		[Test]
//		public void aaaaaaaaaa4aa__()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)/Products(1)/Id", SegmentOp.View, model);
//
			// assert last segment is product name
//		}
//
//		[Test]
//		public void aaaaaaaaaa5()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/suppliers(1)/Address", SegmentOp.View, model);
//
			// Assert last segment value is 'complex value with 3 nodes'
//		}
//
//		[Test]
//		public void aaaaaaaaaa6()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/suppliers(1)/Address/Street", SegmentOp.View, model);
//
			// Assert last segment value is ''
//		}
//
//		[Test]
//		public void aaaaaaaaaa7()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/products(1)/Catalog/", SegmentOp.View, model);
//
			// Assert last segment value is single catalog
//		}
//
//		[Test]
//		public void aaaaaaaaaa8()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//
//			Process("/products(1)/Catalog/Name", SegmentOp.View, model);
//
			// Assert last segment value is single value = name
//		}
//
//		[Test]
//		public void InvalidId_ForResourceMultiResult_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)/Products(1000)/", SegmentOp.View, model);
//
			// assert last segment is product name
//		}
//		
//
//		[Test]
//		public void InvalidId_ForResourceMultiResultPlusPrimitiveProperty_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//
//			Process("/catalogs(1)/Products(1000)/Name", SegmentOp.View, model);
//
			// assert last segment is product name
//		}

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
