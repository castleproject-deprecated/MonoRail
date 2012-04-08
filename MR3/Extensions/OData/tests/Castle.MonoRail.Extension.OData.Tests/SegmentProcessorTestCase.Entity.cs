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
//		static readonly SyndicationPerson EmptyPerson = new SyndicationPerson(null, string.Empty, null);
//		static readonly XmlQualifiedName QualifiedNullAttribute = new XmlQualifiedName("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
//		static readonly XmlQualifiedName QualifiedDataWebPrefix = new XmlQualifiedName("d", "http://www.w3.org/2000/xmlns/");
//		static readonly XmlQualifiedName QualifiedDataWebMetadataPrefix = new XmlQualifiedName("m", "http://www.w3.org/2000/xmlns/");

//		[Test]
//		public void aaaaaaaaaaaaa()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//
//			var content = XmlSyndicationContent.CreateXmlContent(
//				XmlReader.Create(new StringReader(
//@"<content type=""application/xml"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
//<m:properties  >
//	<d:Id m:type=""Edm.Int32"">1</d:Id>
//	<d:Name>test</d:Name>
//</m:properties></content>"), new XmlReaderSettings() ));
//
//			var entry = new SyndicationItem("", content, null, null, DateTimeOffset.Now);
//			entry.AttributeExtensions.Add(QualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices");
//			entry.AttributeExtensions.Add(QualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
//			entry.Id = String.Empty;
//			entry.Title = new TextSyndicationContent(string.Empty);
//			entry.AttributeExtensions[QualifiedNullAttribute] = "true";
//			entry.Authors.Add(EmptyPerson);
//			
//
//			var buffer = new StringBuilder();
//			var writer = XmlWriter.Create(new StringWriter(buffer), 
//										  new XmlWriterSettings() { 
//											  CheckCharacters = false,
//											  ConformanceLevel = ConformanceLevel.Fragment,
//											  Encoding = Encoding.UTF8,
//											  Indent = true,
//											  NewLineHandling = NewLineHandling.Entitize });
//			entry.SaveAsAtom10(writer); writer.Flush();
//			
//			Console.WriteLine(buffer);
//
//			Process("/catalogs", SegmentOp.Create, model);
//		}

		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]


		[Test]
		public void EntitySet_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			
			Process("/catalogs/", SegmentOp.View, model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);
		}

		[Test]
		public void EntitySet_WithResourceProperty_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			
			Process("/products/", SegmentOp.View, model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);
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
		}

		[Test]
		public void EntitySet_WithComplexType_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set).
						AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Xhtml, false));
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/suppliers/", SegmentOp.View, model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);
		}

		// todo: tests for all primitive types

		[Test]
		public void EntityType_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			
			Process("/catalogs(1)/", SegmentOp.View, model);

			Console.WriteLine(_body.ToString());
			var entry = SyndicationItem.Load(XmlReader.Create(new StringReader(_body.ToString())));
			entry.Should().NotBeNull();
		}

//		[Test]
//		public void aaaaaaaaaa12()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1)", SegmentOp.View, model);
//		}
//
//		[Test]
//		public void InvalidId_ForEntityType_()
//		{
//			var model = new StubModel(
//				m =>
//				{
//					m.EntitySet("catalogs", _catalog1Set);
//					m.EntitySet("products", _product1Set);
//					m.EntitySet("suppliers", _supplier1Set);
//				});
//			
//			Process("/catalogs(1000)/", SegmentOp.View, model);
//		}
	}
}
