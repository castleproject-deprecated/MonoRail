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
			segment.Identifier.Should().Be("catalogs");
			segment.Container.Should().BeSameAs(model.ResourceSets.ElementAt(0));
		}

		public class Catalog
		{
			[Key]
			public int Id { get; set; }
		}
	}
}
