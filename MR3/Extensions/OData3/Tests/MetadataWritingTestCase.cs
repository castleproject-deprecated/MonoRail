namespace Castle.MonoRail.Extension.OData3.Tests
{
    using System;
    using FluentAssertions;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using NUnit.Framework;

	[TestFixture]
    public class MetadataWritingTestCase : ODataTestCommon
    {
        private ODataMessageWriter _writer;

        [Test]
        public void empty_edm_model_writes_empty_metadata()
        {
            var edmModel = new EdmModel();
            var response = new StubODataResponse();
            var settings = 
                CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
            _writer = new ODataMessageWriter(response, settings, edmModel);

            _writer.WriteMetadataDocument();

            response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml;charset=utf-8
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" />
</edmx:Edmx>");
        }

        [Test]
        public void edm_model_with_EdmEntityContainer_writes_empty_container()
        {
            var edmModel = new EdmModel();
            edmModel.AddElement(new EdmEntityContainer("namespace", "name"));
            var response = new StubODataResponse();
            var settings =
                CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
            _writer = new ODataMessageWriter(response, settings, edmModel);

            _writer.WriteMetadataDocument();

            response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml;charset=utf-8
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""namespace"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityContainer Name=""name"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

        [Test]
        public void edm_model_with_EdmEntityType_writes_type_and_set()
        {
            var edmModel = new EdmModel();
            var container = new EdmEntityContainer("namespace", "name");
            var productEntityType = new EdmEntityType("namespace", "Product");
            edmModel.AddElement(productEntityType);
            container.AddEntitySet("Products", productEntityType);
            edmModel.AddElement(container);
            var response = new StubODataResponse();
            var settings =
                CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
            _writer = new ODataMessageWriter(response, settings, edmModel);

            _writer.WriteMetadataDocument();

            Console.WriteLine(response.ToString());

            response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml;charset=utf-8
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""namespace"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"" />
      <EntityContainer Name=""name"">
        <EntitySet Name=""Products"" EntityType=""namespace.Product"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

        
    }
}
