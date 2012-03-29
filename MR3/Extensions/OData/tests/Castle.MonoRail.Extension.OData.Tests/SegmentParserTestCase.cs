namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using System.Web;
	using FluentAssertions;
	using NUnit.Framework;


	public static class Asserts
	{
		public static void FirstSegmentIsServiceDirectory(SegmentParser.UriSegment[] segments)
		{
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).IsServiceDirectory.Should().BeTrue();
		}

		public static void FirstSegmentIsMetadata(SegmentParser.UriSegment[] segments)
		{
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).IsMeta.Should().BeTrue();
			segments.ElementAt(0).As<SegmentParser.UriSegment.Meta>().item.IsMetadata.Should().BeTrue();
		}
	}

	
	[TestFixture]
	public class SegmentParserTestCase
	{
		[SetUp]
		public void Init()
		{
		}

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
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog>().AsQueryable());
				}
			);
			var segments = SegmentParser.parse("/catalogs", String.Empty, model);
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			var segment = segments.ElementAt(0);
//			segment.Kind.Should().Be(SegmentKind.Resource);
//			segment.Key.Should().BeNull();
//			segment.Identifier.Should().Be("catalogs");
//			segment.Container.Name.Should().Be("catalogs");
		}

//		[Test]
//		public void AccessingEntityByKey_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
//				}
//			);
//			var segments = SegmentParser.parse("/catalogs(1)", model);
//			segments.Should().NotBeNull();
//			segments.Should().HaveCount(1);
//			var segment = segments.ElementAt(0);
//			segment.Kind.Should().Be(SegmentKind.Resource);
//			segment.Identifier.Should().Be("catalogs(1)");
//			segment.Key.Should().Be("1");
//			segment.Container.Name.Should().Be("catalogs");
//		}
//
//		[Test]
//		public void AccessingEntityAndProp_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
//				}
//			);
//			var segments = SegmentParser.parse("/catalogs(1)/Id", model);
//			segments.Should().NotBeNull();
//			segments.Should().HaveCount(2);
//			var segment1 = segments.ElementAt(0);
//			segment1.Kind.Should().Be(SegmentKind.Resource);
//			segment1.Identifier.Should().Be("catalogs(1)");
//			segment1.Key.Should().Be("1");
//			segment1.Container.Name.Should().Be("catalogs");
//
//			var segment2 = segments.ElementAt(1);
//			segment2.Kind.Should().Be(SegmentKind.Primitive);
//			segment2.Identifier.Should().Be("Id");
//			segment2.Key.Should().BeNull();
//			segment2.Container.Should().BeNull();
//		}
//
//		[Test]
//		public void AccessingEntityAndPropValue_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
//				}
//			);
//			var segments = SegmentParser.parse("/catalogs(1)/Id/$value", model);
//			segments.Should().NotBeNull();
//			segments.Should().HaveCount(3);
//			var segment1 = segments.ElementAt(0);
//			segment1.Kind.Should().Be(SegmentKind.Resource);
//			segment1.Identifier.Should().Be("catalogs(1)");
//			segment1.Key.Should().Be("1");
//			segment1.Container.Name.Should().Be("catalogs");
//
//			var segment2 = segments.ElementAt(1);
//			segment2.Kind.Should().Be(SegmentKind.Primitive);
//			segment2.Identifier.Should().Be("Id");
//			segment2.Key.Should().BeNull();
//			segment2.Container.Should().BeNull();
//
//			var segment3 = segments.ElementAt(2);
//			segment3.Kind.Should().Be(SegmentKind.PrimitiveValue);
//			segment3.Identifier.Should().Be("$value");
//			segment3.Key.Should().BeNull();
//			segment3.Container.Should().BeNull();
//		}

		public class Catalog
		{
			[Key]
			public int Id { get; set; }
		}
	}
}
