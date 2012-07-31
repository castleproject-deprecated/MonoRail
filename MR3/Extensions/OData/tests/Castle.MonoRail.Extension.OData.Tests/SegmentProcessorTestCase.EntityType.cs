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
		[Test]
		public void EntityType_View_Atom_Atom_Success()
		{
			Process("/catalogs(1)/", SegmentOp.View, _model);

			Console.WriteLine(_body.ToString());
			var entry = SyndicationItem.Load(XmlReader.Create(new StringReader(_body.ToString())));
			entry.Should().NotBeNull();

			Process("/catalogs(1)", SegmentOp.View, _model);
			entry = SyndicationItem.Load(XmlReader.Create(new StringReader(_body.ToString())));
			entry.Should().NotBeNull();

			_accessSingle.Should().HaveCount(1);
		}

		[Test]
		public void EntityType_View_Atom_Atom_Success2()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/suppliers(1)/", SegmentOp.View, model);

			Console.WriteLine(_body.ToString());
			var entry = SyndicationItem.Load(XmlReader.Create(new StringReader(_body.ToString())));
			entry.Should().NotBeNull();

			_accessSingle.Should().HaveCount(1);
		}

		[Test, ExpectedException(ExpectedMessage = "Lookup of entity catalogs for key 1000 failed.")]
		public void EntityType_NonExistinEntityById()
		{
			// TODO: this should return a xml response with the error details
			Process("/catalogs(1000)/", SegmentOp.View, _model);
		}
	}
}
