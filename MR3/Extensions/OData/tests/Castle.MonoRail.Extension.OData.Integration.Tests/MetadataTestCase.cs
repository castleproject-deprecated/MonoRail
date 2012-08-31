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
    }
}
