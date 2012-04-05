namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.IO;
	using System.ServiceModel.Syndication;
	using System.Text;
	using System.Xml;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		static readonly XmlQualifiedName QualifiedNullAttribute = new XmlQualifiedName("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
		static readonly SyndicationPerson EmptyPerson = new SyndicationPerson(null, string.Empty, null);
		static readonly XmlQualifiedName QualifiedDataWebPrefix = new XmlQualifiedName("d", "http://www.w3.org/2000/xmlns/");
		static readonly XmlQualifiedName QualifiedDataWebMetadataPrefix = new XmlQualifiedName("m", "http://www.w3.org/2000/xmlns/");

		[Test]
		public void aaaaaaaaaaaaa()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs", String.Empty, model);

			var content = XmlSyndicationContent.CreateXmlContent(
				XmlReader.Create(new StringReader(
@"<content type=""application/xml"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
<m:properties  >
	<d:Id m:type=""Edm.Int32"">1</d:Id>
	<d:Name>test</d:Name>
</m:properties></content>"), new XmlReaderSettings() ));

			var entry = new SyndicationItem("", content, null, null, DateTimeOffset.Now);
			entry.AttributeExtensions.Add(QualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices");
			entry.AttributeExtensions.Add(QualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
			entry.Id = String.Empty;
			entry.Title = new TextSyndicationContent(string.Empty);
			entry.AttributeExtensions[QualifiedNullAttribute] = "true";
			entry.Authors.Add(EmptyPerson);
			
			var buffer = new StringBuilder();
			var writer = XmlWriter.Create(new StringWriter(buffer), 
										  new XmlWriterSettings() { 
											  CheckCharacters = false,
											  ConformanceLevel = ConformanceLevel.Fragment,
											  Encoding = Encoding.UTF8,
											  Indent = true,
											  NewLineHandling = NewLineHandling.Entitize });
			entry.SaveAsAtom10(writer); writer.Flush();
			
			Console.WriteLine(buffer);

			SegmentProcessor.Process(SegmentOp.Create, segments, new RequestParameters(model, "application/atom+xml", new MemoryStream(Encoding.UTF8.GetBytes(buffer.ToString()))));


		}

		[Test]
		public void aaaaaaaaaa()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs/", String.Empty, model);

			SegmentProcessor.Process(SegmentOp.View, segments, new RequestParameters(model, "", null));
		}

		[Test]
		public void aaaaaaaaaa2()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/", String.Empty, model);

			SegmentProcessor.Process(SegmentOp.View, segments, new RequestParameters(model, "", null));
		}

		[Test]
		public void aaaaaaaaaa12()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)", String.Empty, model);

			SegmentProcessor.Process(SegmentOp.View, segments, new RequestParameters(model, "", null));
		}

		[Test]
		public void InvalidId_ForEntityType_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1000)/", String.Empty, model);

			SegmentProcessor.Process(SegmentOp.View, segments, new RequestParameters(model, "", null));
		}
	}
}
