namespace Castle.MonoRail.Extension.OData.Integration.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using FluentAssertions;
    using MonoRail.Integration.Tests;
    using NUnit.Framework;

    [TestFixture, Category("Integration")]
    public class MetadataTestCase : BaseServerTest
	{
        public MetadataTestCase()
		{
			this.WebSiteFolder = "ODataTestWebSite";
		}

        [Test]
        public void metadata_for_simple_model()
        {
            var req =
                (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/SingleEntitySetModel/$metadata")));
            req.Accept = "*/*";
            req.Method = "GET";
            var reply = (HttpWebResponse) req.GetResponse();
            reply.StatusCode.Should().Be(HttpStatusCode.OK);
            reply.ContentType.Should().Be("application/xml");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""ns"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Vendor"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""F1"" Type=""Edm.SByte"" Nullable=""false"" />
        <Property Name=""F2"" Type=""Edm.Byte"" Nullable=""false"" />
        <Property Name=""F3"" Type=""Edm.Int16"" Nullable=""false"" />
        <Property Name=""F4"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""F5"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""F6"" Type=""Edm.Single"" Nullable=""false"" />
        <Property Name=""F7"" Type=""Edm.Double"" Nullable=""false"" />
        <Property Name=""F8"" Type=""Edm.Decimal"" Nullable=""false"" />
        <Property Name=""F9"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""F10"" Type=""Edm.Binary"" />
      </EntityType>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Vendors"" EntityType=""ns.Vendor"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

        [Test]
        public void metadata_for_model_with_enums()
        {
            var req =
                (HttpWebRequest) WebRequest.CreateDefault(new Uri(BuildUrl("/models/Repo/$metadata")));
            req.Accept = "*/*";
            req.Method = "GET";
            var reply = (HttpWebResponse) req.GetResponse();
            reply.StatusCode.Should().Be(HttpStatusCode.OK);
            reply.ContentType.Should().Be("application/xml");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(@"");
        }


        [Test]
        public void metadata_for_root_model()
        {
            var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/RootModel/$metadata")));
            req.Accept = "*/*";
            req.Method = "GET";
            var reply = (HttpWebResponse)req.GetResponse();
            reply.StatusCode.Should().Be(HttpStatusCode.OK);
            reply.ContentType.Should().Be("application/xml");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""ns"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Repository"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Branches"" Relationship=""ns.ns_Repository_Branches_ns_Branch_Repository"" ToRole=""Branches"" FromRole=""Repository"" />
      </EntityType>
      <EntityType Name=""Revision"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""FileName"" Type=""Edm.String"" />
        <Property Name=""UserId"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Branch"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Revisions"" Relationship=""ns.ns_Revision_Branch_ns_Branch_Revisions"" ToRole=""Revisions"" FromRole=""Branch"" />
      </EntityType>
      <Association Name=""ns_Repository_Branches_ns_Branch_Repository"">
        <End Type=""ns.Branch"" Role=""Branches"" Multiplicity=""*"" />
        <End Type=""ns.Repository"" Role=""Repository"" Multiplicity=""*"" />
      </Association>
      <Association Name=""ns_Revision_Branch_ns_Branch_Revisions"">
        <End Type=""ns.Branch"" Role=""Branch"" Multiplicity=""*"" />
        <End Type=""ns.Revision"" Role=""Revisions"" Multiplicity=""*"" />
      </Association>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Repositories"" EntityType=""ns.Repository"" />
        <EntitySet Name=""Revisions"" EntityType=""ns.Revision"" />
        <EntitySet Name=""Branches"" EntityType=""ns.Branch"" />
        <AssociationSet Name=""ns_Repository_Branches_ns_Branch_RepositorySet"" Association=""ns.ns_Repository_Branches_ns_Branch_Repository"">
          <End Role=""Repository"" EntitySet=""Repositories"" />
          <End Role=""Branches"" EntitySet=""Branches"" />
        </AssociationSet>
        <AssociationSet Name=""ns_Revision_Branch_ns_Branch_RevisionsSet"" Association=""ns.ns_Revision_Branch_ns_Branch_Revisions"">
          <End Role=""Branch"" EntitySet=""Branches"" />
          <End Role=""Revisions"" EntitySet=""Revisions"" />
        </AssociationSet>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

        [Test]
        public void metadata_for_root2_model()
        {
            var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/RootModel2/$metadata")));
            req.Accept = "*/*";
            req.Method = "GET";
            var reply = (HttpWebResponse)req.GetResponse();
            reply.StatusCode.Should().Be(HttpStatusCode.OK);
            reply.ContentType.Should().Be("application/xml");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""ns"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Repository"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Branches"" Relationship=""ns.ns_Repository_Branches_ns_Branch_Repository"" ToRole=""Branches"" FromRole=""Repository"" />
      </EntityType>
      <EntityType Name=""Branch"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Revisions"" Relationship=""ns.ns_Revision_Branch_ns_Branch_Revisions"" ToRole=""Revisions"" FromRole=""Branch"" />
      </EntityType>
      <EntityType Name=""Revision"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""FileName"" Type=""Edm.String"" />
        <Property Name=""UserId"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <Association Name=""ns_Repository_Branches_ns_Branch_Repository"">
        <End Type=""ns.Branch"" Role=""Branches"" Multiplicity=""*"" />
        <End Type=""ns.Repository"" Role=""Repository"" Multiplicity=""*"" />
      </Association>
      <Association Name=""ns_Revision_Branch_ns_Branch_Revisions"">
        <End Type=""ns.Branch"" Role=""Branch"" Multiplicity=""*"" />
        <End Type=""ns.Revision"" Role=""Revisions"" Multiplicity=""*"" />
      </Association>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Repositories"" EntityType=""ns.Repository"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }

        [Test]
        public void metadata_for_self_relating_model()
        {
            var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/HierarchicalModel/$metadata")));
            req.Accept = "*/*";
            req.Method = "GET";
            var reply = (HttpWebResponse)req.GetResponse();
            reply.StatusCode.Should().Be(HttpStatusCode.OK);
            reply.ContentType.Should().Be("application/xml");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:DataServices m:DataServiceVersion=""3.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
    <Schema Namespace=""ns"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
      <EntityType Name=""Category"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <NavigationProperty Name=""Parent"" Relationship=""ns.ns_Category_Category_ns_Category_Parent"" ToRole=""Parent"" FromRole=""Category"" />
        <NavigationProperty Name=""Children"" Relationship=""ns.ns_Category_Category_ns_Category_Children"" ToRole=""Children"" FromRole=""Category"" />
      </EntityType>
      <Association Name=""ns_Category_Category_ns_Category_Parent"">
        <End Type=""ns.Category"" Role=""Category"" Multiplicity=""*"" />
        <End Type=""ns.Category"" Role=""Parent"" Multiplicity=""0..1"" />
      </Association>
      <Association Name=""ns_Category_Category_ns_Category_Children"">
        <End Type=""ns.Category"" Role=""Category"" Multiplicity=""0..1"" />
        <End Type=""ns.Category"" Role=""Children"" Multiplicity=""*"" />
      </Association>
      <EntityContainer Name=""container"">
        <EntitySet Name=""Categories"" EntityType=""ns.Category"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
        }


    }
}
