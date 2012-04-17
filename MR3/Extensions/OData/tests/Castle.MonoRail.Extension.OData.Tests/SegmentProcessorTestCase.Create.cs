namespace Castle.MonoRail.Extension.OData.Tests
{
    using System;
    using System.Data.Services.Common;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Xml;
    using FluentAssertions;
    using NUnit.Framework;

	static class SyndicationExtensions
	{
		public static Stream ToStream(this SyndicationItem item)
		{
			var stream = new MemoryStream();
			var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
			item.SaveAsAtom10(writer);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}


    public partial class SegmentProcessorTestCase
    {
        // naming convention for testing methods
        // [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test, Description("The EntityContainer only has Catalog, so creation is for nested object")]
		public void PropCollection_Create_Atom_Atom_Success()
		{
			var prod = new Product1() { Created = DateTime.Now, Modified = DateTime.Now, IsCurated = true, Name = "testing", Price = 2.3m };
			prod.ToSyndicationItem();

			Process("/catalogs(1)/Products/", SegmentOp.Create, _modelWithMinimalContainer, inputStream: prod.ToSyndicationItem().ToStream() );



//			Console.WriteLine(_body.ToString());
//			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
//			feed.Should().NotBeNull();
//
//			feed.Id.Should().BeEquivalentTo("http://localhost/base/catalogs(1)/products");
//			feed.Items.Should().HaveCount(2);
//
//			feed.Items.ElementAt(0).Id.Should().BeEquivalentTo("http://localhost/base/catalogs(1)/products(1)");
//			feed.Items.ElementAt(1).Id.Should().BeEquivalentTo("http://localhost/base/catalogs(1)/products(2)");
		}

        [Test]
        public void EntitySet_Create_Atom_Atom_Success()
        {


        	// var dict = new AtomSerialization.ContentDict();



//
//
//            const string content = 
//@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
//<entry xml:base=""http://localhost/base/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">
//  <id>http://localhost/base/suppliers(1)</id>
//  <title type=""text""></title>
//  <updated>2012-04-08T08:52:45Z</updated>
//  <author>
//    <name />
//  </author>
//  <link rel=""edit"" title=""Supplier1"" href=""suppliers(1)"" />
//  <category term=""TestNamespace.Supplier1"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
//  <content type=""application/xml"">
//    <m:properties>
//      <d:Id m:type=""Edm.Int32"">1</d:Id>
//      <d:Address m:type=""TestNamespace.Address1"">
//        <d:Street>wilson ave</d:Street>
//        <d:Zip>vxxxx</d:Zip>
//        <d:Country>canada</d:Country>
//      </d:Address>
//    </m:properties>
//  </content>
//</entry>";
//
//            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
//
//            Process("/suppliers", SegmentOp.Create, _model, inputStream: stream);
//
//        	_created.Should().HaveCount(1);
//			Assert.AreSame(_created[0].Item1, _model.GetResourceType("Supplier1").Value);

        	// Assert deserialization worked
        	// var element = (Supplier1) _created[0].Item2;
        	// element.Id.Should().Be(1);
        	// element.Address.Street.Should().Be("wilson ave");
        	// element.Address.Zip.Should().Be("vxxxx");
        	// element.Address.Country.Should().Be("canada");
        }


    }
}
