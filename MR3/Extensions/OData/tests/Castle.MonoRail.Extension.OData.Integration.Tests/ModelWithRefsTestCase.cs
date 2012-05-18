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
	/// The server side model has no direct relation to the resource navigation 
	/// </summary>
	[TestFixture, Category("Integration")]
	public class ModelWithRefsTestCase : BaseServerTest
	{
		public ModelWithRefsTestCase()
		{
			this.WebSiteFolder = "ODataTestWebSite";
		}

//		[Test]
//		public void FetchingRepositories()
//		{
//			
//		}
//
//		[Test]
//		public void FetchingRepositories_ExpandingOnBranches()
//		{
//
//		}
//
//		[Test]
//		public void FetchingRepositories_ExpandingOnBranchesAndRevisions()
//		{
//
//		}
//
//		[Test]
//		public void Create_Repository()
//		{
//			var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(BuildUrl("/models/Repo/Repositories")));
//
//			req.Accept = "application/json";
//			req.ContentType = "application/json";
//			req.Method = "POST";
//			var reqWriter = new StreamWriter(req.GetRequestStream());
//
//			reqWriter.Write(
//@"{
//  ""d"": {
//    ""Name"": ""Repo 2""
//  }
//}");
//			reqWriter.Flush();
//
//			var reply = (HttpWebResponse)req.GetResponse();
//			reply.StatusCode.Should().Be(HttpStatusCode.Created);
//			reply.ContentType.Should().Be("application/json; charset=utf-8");
//			var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
//
//			Console.WriteLine(replyContent);
//		}
//
//		[Test]
//		public void Update_Repository()
//		{
//
//		}
//
//		[Test]
//		public void Create_Branch()
//		{
//
//		}
//
//		[Test]
//		public void Update_Branch()
//		{
//
//		}




		[Test, Description("Since this model uses indirection types, the metadata should not reflect those")]
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
