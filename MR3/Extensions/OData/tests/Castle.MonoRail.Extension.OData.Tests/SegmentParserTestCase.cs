namespace Castle.MonoRail.Extension.OData.Tests
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using System.Web;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class SegmentParserTestCase
	{
		private SegmentParser parser;

		[SetUp]
		public void Init()
		{
			parser = new SegmentParser();
		}

		[Test]
		public void ServiceDirectory_()
		{
			var segments = parser.ParseAndBind("/", new StubModel(null));
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).Kind.Should().Be(SegmentKind.ServiceDirectory);
		}

		[Test]
		public void ServiceDirectory_2()
		{
			var segments = parser.ParseAndBind("", new StubModel(null));
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).Kind.Should().Be(SegmentKind.ServiceDirectory);
		}

		[Test]
		public void MetadataIdentifier_()
		{
			var segments = parser.ParseAndBind("/$metadata", new StubModel(null));
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).Kind.Should().Be(SegmentKind.Metadata);
		}

		[Test]
		public void MetadataIdentifier_2()
		{
			var segments = parser.ParseAndBind("/$metadata", new StubModel(null));
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			segments.ElementAt(0).Kind.Should().Be(SegmentKind.Metadata);
		}

		[Test, ExpectedException(typeof(HttpException), ExpectedMessage = "catalogs does not map to a known entity")]
		public void AccessingEntity_ThatDoesNotExists_HttpError()
		{
			parser.ParseAndBind("/catalogs", new StubModel(null));
		}

		[Test]
		public void AccessingEntity_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
				}
			);
			var segments = parser.ParseAndBind("/catalogs", model);
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			var segment = segments.ElementAt(0);
			segment.Kind.Should().Be(SegmentKind.Resource);
			segment.Key.Should().BeNull();
			segment.Identifier.Should().Be("catalogs");
			segment.Container.Name.Should().Be("catalogs");
		}

		[Test]
		public void AccessingEntityByKey_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
				}
			);
			var segments = parser.ParseAndBind("/catalogs(1)", model);
			segments.Should().NotBeNull();
			segments.Should().HaveCount(1);
			var segment = segments.ElementAt(0);
			segment.Kind.Should().Be(SegmentKind.Resource);
			segment.Identifier.Should().Be("catalogs(1)");
			segment.Key.Should().Be("1");
			segment.Container.Name.Should().Be("catalogs");
		}

		[Test]
		public void AccessingEntityAndProp_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
				}
			);
			var segments = parser.ParseAndBind("/catalogs(1)/Id", model);
			segments.Should().NotBeNull();
			segments.Should().HaveCount(2);
			var segment1 = segments.ElementAt(0);
			segment1.Kind.Should().Be(SegmentKind.Resource);
			segment1.Identifier.Should().Be("catalogs(1)");
			segment1.Key.Should().Be("1");
			segment1.Container.Name.Should().Be("catalogs");

			var segment2 = segments.ElementAt(1);
			segment2.Kind.Should().Be(SegmentKind.Primitive);
			segment2.Identifier.Should().Be("Id");
			segment2.Key.Should().BeNull();
			segment2.Container.Should().BeNull();
		}

		[Test]
		public void AccessingEntityAndPropValue_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", new List<Catalog>().AsQueryable(), EntitySetPermission.ReadOnly);
				}
			);
			var segments = parser.ParseAndBind("/catalogs(1)/Id/$value", model);
			segments.Should().NotBeNull();
			segments.Should().HaveCount(3);
			var segment1 = segments.ElementAt(0);
			segment1.Kind.Should().Be(SegmentKind.Resource);
			segment1.Identifier.Should().Be("catalogs(1)");
			segment1.Key.Should().Be("1");
			segment1.Container.Name.Should().Be("catalogs");

			var segment2 = segments.ElementAt(1);
			segment2.Kind.Should().Be(SegmentKind.Primitive);
			segment2.Identifier.Should().Be("Id");
			segment2.Key.Should().BeNull();
			segment2.Container.Should().BeNull();

			var segment3 = segments.ElementAt(2);
			segment3.Kind.Should().Be(SegmentKind.PrimitiveValue);
			segment3.Identifier.Should().Be("$value");
			segment3.Key.Should().BeNull();
			segment3.Container.Should().BeNull();
		}

		public class Catalog
		{
			[Key]
			public int Id { get; set; }
		}
	}
}
