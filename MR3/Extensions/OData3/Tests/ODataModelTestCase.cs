using FluentAssertions;

namespace Castle.MonoRail.Extension.OData3.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Microsoft.Data.OData;
    using NUnit.Framework;

    [TestFixture]
    public class ODataModelTestCase : ODataTestCommon
    {
        [Test]
        public void for_simple_type_generate_keys_and_primitive_properties()
        {
            var model = new Models.SimpleODataModel();
            model.InitializeModels(null);

            var response = new StubODataResponse();
            var settings =
                CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
            var writer = new ODataMessageWriter(response, settings, model.EdmModel);

            writer.WriteMetadataDocument();

            Console.WriteLine(response.ToString());

	        response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""schemaNs"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
      </EntityType>
      <EntityContainer Name=""containerName"">
        <EntitySet Name=""Products"" EntityType=""schemaNs.Product"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

		[Test]
		public void model_with_bi_and_uni_directional_relations()
		{
			var model = new Models.ModelWithAssociation();
			model.InitializeModels(null);

			var response = new StubODataResponse();
			var settings =
				CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
			var writer = new ODataMessageWriter(response, settings, model.EdmModel);

			writer.WriteMetadataDocument();

			Console.WriteLine(response.ToString());

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/xml
<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""schemaNs"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Product"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Categories"" Relationship=""schemaNs.schemaNs_Product_Categories_schemaNs_Category_Product"" ToRole=""Categories"" FromRole=""Product"" />
      </EntityType>
      <EntityType Name=""Category"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""ProductParent"" Relationship=""schemaNs.schemaNs_Product_Category_schemaNs_Category_ProductParent"" ToRole=""ProductParent"" FromRole=""Category"" />
        <NavigationProperty Name=""Parent"" Relationship=""schemaNs.schemaNs_Category_Category_schemaNs_Category_Parent"" ToRole=""Parent"" FromRole=""Category"" />
      </EntityType>
      <Association Name=""schemaNs_Product_Categories_schemaNs_Category_Product"">
        <End Type=""schemaNs.Category"" Role=""Categories"" Multiplicity=""*"" />
        <End Type=""schemaNs.Product"" Role=""Product"" Multiplicity=""*"" />
      </Association>
      <Association Name=""schemaNs_Product_Category_schemaNs_Category_ProductParent"">
        <End Type=""schemaNs.Category"" Role=""Category"" Multiplicity=""*"" />
        <End Type=""schemaNs.Product"" Role=""ProductParent"" Multiplicity=""0..1"" />
      </Association>
      <Association Name=""schemaNs_Category_Category_schemaNs_Category_Parent"">
        <End Type=""schemaNs.Category"" Role=""Category"" Multiplicity=""*"" />
        <End Type=""schemaNs.Category"" Role=""Parent"" Multiplicity=""0..1"" />
      </Association>
      <EntityContainer Name=""containerName"">
        <EntitySet Name=""Products"" EntityType=""schemaNs.Product"" />
        <EntitySet Name=""Categories"" EntityType=""schemaNs.Category"" />
        <AssociationSet Name=""schemaNs_Product_Categories_schemaNs_Category_ProductSet"" Association=""schemaNs.schemaNs_Product_Categories_schemaNs_Category_Product"">
          <End Role=""Product"" EntitySet=""Products"" />
          <End Role=""Categories"" EntitySet=""Categories"" />
        </AssociationSet>
        <AssociationSet Name=""schemaNs_Product_Category_schemaNs_Category_ProductParentSet"" Association=""schemaNs.schemaNs_Product_Category_schemaNs_Category_ProductParent"">
          <End Role=""Category"" EntitySet=""Categories"" />
          <End Role=""ProductParent"" EntitySet=""Products"" />
        </AssociationSet>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
		}
    }
}
