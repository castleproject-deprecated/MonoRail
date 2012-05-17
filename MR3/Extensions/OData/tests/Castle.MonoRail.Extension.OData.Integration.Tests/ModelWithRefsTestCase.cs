namespace Castle.MonoRail.Extension.OData.Integration.Tests
{
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using FluentAssertions;
	using MonoRail.Integration.Tests;
	using NUnit.Framework;

	/// <summary>
	/// The server side model has no direct relation to the navigation 
	/// </summary>
	[TestFixture, Category("Integration")]
	public class ModelWithRefsTestCase : BaseServerTest
	{
		public ModelWithRefsTestCase()
		{
			this.WebSiteFolder = "ODataTestWebSite";
		}

		[Test]
		public void CorrectMetadataGenerated()
		{
			var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/Repo/$metadata")));

			var rep = req.GetResponse();
			var reader = new StreamReader(rep.GetResponseStream(), Encoding.UTF8);
			reader.ReadToEnd().Should().BeEquivalentTo(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""2.0"">
    <Schema Namespace=""ns"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://schemas.microsoft.com/ado/2008/09/edm"">
      <EntityType Name=""Repository"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" Nullable=""true"" />
        <NavigationProperty Name=""Branches"" Relationship=""ns.Repository_Branches"" FromRole=""Repository"" ToRole=""Branches"" />
      </EntityType>
      <EntityType Name=""Branch"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" Nullable=""true"" />
        <NavigationProperty Name=""Revisions"" Relationship=""ns.Branch_Revisions"" FromRole=""Branch"" ToRole=""Revisions"" />
      </EntityType>
      <EntityType Name=""Revision"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""FileName"" Type=""Edm.String"" Nullable=""true"" />
        <Property Name=""UserId"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <Association Name=""Repository_Branches"">
        <End Role=""Repository"" Type=""ns.Repository"" Multiplicity=""*"" />
        <End Role=""Branches"" Type=""ns.Branch"" Multiplicity=""*"" />
      </Association>
      <Association Name=""Branch_Revisions"">
        <End Role=""Branch"" Type=""ns.Branch"" Multiplicity=""*"" />
        <End Role=""Revisions"" Type=""ns.Revision"" Multiplicity=""*"" />
      </Association>
      <EntityContainer Name=""container"" m:IsDefaultEntityContainer=""true"">
        <EntitySet Name=""Repositories"" EntityType=""ns.Repository"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>");
		}


	}
}
