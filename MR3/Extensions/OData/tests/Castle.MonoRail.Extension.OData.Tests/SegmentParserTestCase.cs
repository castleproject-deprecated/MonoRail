namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using System.Web;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class SegmentParserTestCase
	{
		[Test]
		public void ServiceDirectory_()
		{
			var segments = SegmentParser.parse("/", string.Empty, new StubModel(null));
			Asserts.FirstSegmentIsServiceDirectory(segments);
		}

		[Test]
		public void ServiceDirectory_2()
		{
			var segments = SegmentParser.parse(string.Empty, string.Empty, new StubModel(null));
			Asserts.FirstSegmentIsServiceDirectory(segments);
		}

		[Test]
		public void MetadataIdentifier_()
		{
			var segments = SegmentParser.parse("/$metadata", String.Empty, new StubModel(null));
			Asserts.FirstSegmentIsMetadata(segments);
		}

		[Test]
		public void MetadataIdentifier_2()
		{
			var segments = SegmentParser.parse("/$metadata", String.Empty, new StubModel(null));
			Asserts.FirstSegmentIsMetadata(segments);
		}

		[Test, ExpectedException(typeof(HttpException), ExpectedMessage = "First segment of uri could not be parsed")]
		public void AccessingEntity_ThatDoesNotExists_HttpError()
		{
			SegmentParser.parse("/catalogs", String.Empty, new StubModel(null));
		}

		[Test]
		public void AccessingEntity_()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable()) 
			);
			var segments = SegmentParser.parse("/catalogs", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 1);
			Asserts.IsEntitySet(segments.ElementAt(0), "catalogs", model.GetResourceType("catalogs").Value);
		}

		[Test]
		public void AccessingEntityByKey_()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			var segments = SegmentParser.parse("/catalogs(1)", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 1);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
		}

		[Test]
		public void AccessingEntityAndProp_()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			var segments = SegmentParser.parse("/catalogs(1)/Id", String.Empty, model);
			
			Asserts.ExpectingSegmentsCount(segments, 2);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Id");
		}
		
		[Test]
		public void AccessingEntityAndProp_2()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			var segments = SegmentParser.parse("/catalogs(1)/Name", String.Empty, model);
			
			Asserts.ExpectingSegmentsCount(segments, 2);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Name");
		}

		[Test, ExpectedException(typeof(HttpException), ExpectedMessage = "Segment does not match a property or operation")]
		public void AccessingEntity_NonExistingProperty()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			SegmentParser.parse("/catalogs(1)/Something", String.Empty, model);
		}

		[Test]
		public void AccessingEntityAndPropValue_()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			var segments = SegmentParser.parse("/catalogs(1)/Id/$value", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 3);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), "Id");
			Asserts.IsMeta_Value(segments.ElementAt(2));
		}

		[Test]
		public void AccessingEntityAndPropValue_2()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable())
			);
			var segments = SegmentParser.parse("/catalogs(1)/Name/$value", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 3);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), "Name");
			Asserts.IsMeta_Value(segments.ElementAt(2));
		}

		[Test]
		public void AccessingEntity_And_OneToManyRelationship_1()
		{
			var model = new StubModel(
				m =>
					{
						m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
						m.EntitySet("products", new List<Product2>().AsQueryable());	
					});
			var segments = SegmentParser.parse("/catalogs(1)/Products/", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 2);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			
			// Not sure which one this should be? Products or catalog? 
			Asserts.IsPropertyCollection(segments.ElementAt(1), Name: "Products", resource: model.GetResourceType("products").Value);
		}

		[Test]
		public void AccessingEntity_And_OneToManyRelationshipWithKey_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
					m.EntitySet("products", new List<Product2>().AsQueryable());
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(2)/", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 2);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Products", key: "2");
		}

		[Test]
		public void AccessingEntity_And_OneToManyRelationshipWithKey_And_Property()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
					m.EntitySet("products", new List<Product2>().AsQueryable());
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(2)/Name", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 3);
			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs", resource: model.GetResourceType("catalogs").Value);
			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Products", key: "2");
			Asserts.IsPropertySingle(segments.ElementAt(2), name: "Name");
		}

		public class Catalog1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class Product2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class Catalog2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product2> Products { get; set; }
		}

		public static class Asserts
		{
			public static void FirstSegmentIsServiceDirectory(SegmentParser.UriSegment[] segments)
			{
				ExpectingSegmentsCount(segments, 1);
				segments.ElementAt(0).IsServiceDirectory.Should().BeTrue();
			}

			public static void FirstSegmentIsMetadata(SegmentParser.UriSegment[] segments)
			{
				ExpectingSegmentsCount(segments, 1);
				segments.ElementAt(0).IsMeta.Should().BeTrue();
				segments.ElementAt(0).As<SegmentParser.UriSegment.Meta>().item.IsMetadata.Should().BeTrue();
			}

			public static void ExpectingSegmentsCount(SegmentParser.UriSegment[] segments, int count)
			{
				segments.Should().NotBeNull();
				segments.Should().HaveCount(count);
			}

			public static void IsEntitySet(SegmentParser.UriSegment seg, string Name, ResourceType resource)
			{
				var segment = seg.As<SegmentParser.UriSegment.EntitySet>();
				segment.Should().NotBeNull();
				segment.item.Key.Should().BeNull();
				segment.item.Name.Should().Be(Name);
				segment.item.ResourceType.Should().Be(resource);
			}

			public static void IsEntityType(SegmentParser.UriSegment seg, string Key, string Name, ResourceType resource)
			{
				var segment = seg.As<SegmentParser.UriSegment.EntityType>();
				segment.Should().NotBeNull();
				segment.item.Key.Should().Be(Key);
				segment.item.Name.Should().Be(Name);
				segment.item.ResourceType.Should().Be(resource);
			}

			public static void IsPropertySingle(SegmentParser.UriSegment elementAt, string name, string key = null)
			{
				var segment = elementAt.As<SegmentParser.UriSegment.PropertyAccessSingle>();
				segment.Should().NotBeNull();
				segment.item.Property.Name.Should().Be(name);
				segment.item.ResourceType.Should().NotBeNull();
				if (key == null )
					segment.item.Key.Should().BeNull();
				else
				{
					segment.item.Key.Should().NotBeNull();
					segment.item.Key.Should().Be(key);
				}
			}

			public static void IsMeta_Value(SegmentParser.UriSegment elementAt)
			{
				var segment = elementAt.As<SegmentParser.UriSegment.Meta>();
				segment.Should().NotBeNull();
				segment.item.IsValue.Should().BeTrue();
			}

			public static void IsPropertyCollection(SegmentParser.UriSegment elementAt, string Name, ResourceType resource)
			{
				var segment = elementAt.As<SegmentParser.UriSegment.PropertyAccessCollection>();
				segment.Should().NotBeNull();
				segment.item.Property.Name.Should().Be(Name);
				segment.item.ResourceType.Should().NotBeNull();
				segment.item.ResourceType.Should().Be(resource);
				segment.item.Key.Should().BeNull();
			}
		}
	}
}
