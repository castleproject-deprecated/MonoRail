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
@"DataServiceVersion 3.0;;Content-Type application/xml
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
@"DataServiceVersion 3.0;;Content-Type application/xml
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

            // Console.WriteLine(response.ToString());

            response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml
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

        [Test]
        public void edm_model_with_enum()
        {
            var model = Models.ModelWithEnums.Build();

            var response = new StubODataResponse();
            var settings =
                CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
            _writer = new ODataMessageWriter(response, settings, model);

            _writer.WriteMetadataDocument();

            Console.WriteLine(response.ToString());

            response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""schema"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""Status"" Type=""schema.StatusType"" Nullable=""false"" />
      </EntityType>
      <EnumType Name=""StatusType"">
        <Member Name=""InStock"" Value=""0"" />
        <Member Name=""PreOrder"" Value=""2"" />
        <Member Name=""BackOrder"" Value=""5"" />
      </EnumType>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Products"" EntityType=""schema.Product"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

		[Test]
		public void edm_model_with_complex_types()
		{
			var model = Models.ModelWithComplexType.Build();
			var response = new StubODataResponse();
			var settings =
				CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
			_writer = new ODataMessageWriter(response, settings, model);

			_writer.WriteMetadataDocument();

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""schema"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""MainAddress"" Type=""schema.Address"" />
        <Property Name=""OtherAddresses"" Type=""Collection(schema.Address)"" />
      </EntityType>
      <ComplexType Name=""Address"">
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""City"" Type=""Edm.String"" />
        <Property Name=""Zip"" Type=""Edm.String"" />
      </ComplexType>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Products"" EntityType=""schema.Product"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
		}

		[Test]
		public void edm_model_with_multipleentitytypes_but_single_entityset()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var response = new StubODataResponse();
			var settings =
				CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
			_writer = new ODataMessageWriter(response, settings, model);

			_writer.WriteMetadataDocument();

			// Console.WriteLine( response.ToString() );

			response.ToString().Should().Be(@"DataServiceVersion 3.0;;Content-Type application/xml
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""schema"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Categories"" Relationship=""schema.schema_Product_Categories_schema_Category_Product"" ToRole=""Categories"" FromRole=""Product"" />
      </EntityType>
      <EntityType Name=""Category"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""ProductParent"" Relationship=""schema.schema_Product_Category_schema_Category_ProductParent"" ToRole=""ProductParent"" FromRole=""Category"" />
        <NavigationProperty Name=""Parent"" Relationship=""schema.schema_Category_Category_schema_Category_Parent"" ToRole=""Parent"" FromRole=""Category"" />
      </EntityType>
      <Association Name=""schema_Product_Categories_schema_Category_Product"">
        <End Type=""schema.Category"" Role=""Categories"" Multiplicity=""*"" />
        <End Type=""schema.Product"" Role=""Product"" Multiplicity=""*"" />
      </Association>
      <Association Name=""schema_Product_Category_schema_Category_ProductParent"">
        <End Type=""schema.Category"" Role=""Category"" Multiplicity=""*"" />
        <End Type=""schema.Product"" Role=""ProductParent"" Multiplicity=""0..1"" />
      </Association>
      <Association Name=""schema_Category_Category_schema_Category_Parent"">
        <End Type=""schema.Category"" Role=""Category"" Multiplicity=""*"" />
        <End Type=""schema.Category"" Role=""Parent"" Multiplicity=""0..1"" />
      </Association>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Products"" EntityType=""schema.Product"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
		}
        
    }
}
