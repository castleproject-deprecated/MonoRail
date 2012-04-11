namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Data.Services.Common;
	using System.IO;
	using System.ServiceModel.Syndication;
	using System.Text;
	using System.Xml;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void EntitySet_View_Atom_Atom_Success()
		{
			Process("/catalogs/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			_accessMany.Should().HaveCount(1);
		}

		[Test]
		public void EntitySet_WithResourceProperty_View_Atom_Atom_Success()
		{
			Process("/products/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			_accessMany.Should().HaveCount(1);
		}

		[Test]
		public void EntitySet_WithPropMappingToTitle_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set).
						AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Xhtml, false));
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/catalogs/", SegmentOp.View, model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			_accessMany.Should().HaveCount(1);
		}

		[Test]
		public void EntitySet_WithComplexType_View_Atom_Atom_Success()
		{
			Process("/suppliers/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			_accessMany.Should().HaveCount(1);
		}

		// todo: tests for all primitive types

	}
}
