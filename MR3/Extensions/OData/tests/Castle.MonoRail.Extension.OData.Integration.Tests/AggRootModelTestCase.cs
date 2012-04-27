namespace Castle.MonoRail.Extension.OData.Integration.Tests
{
	using System;
	using System.IO;
	using System.Net;
	using Castle.MonoRail.Integration.Tests;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture, Category("Integration")]
	public class ActionResultsIntegrationTestCase : BaseServerTest
	{
		public ActionResultsIntegrationTestCase()
		{
			this.WebSiteFolder = "ODataTestWebSite";
		}

		[Test, Ignore("Need to dig into this later")]
		public void Post_Repository_ExpectsSuccessfulCreation()
		{
			var req = (HttpWebRequest) WebRequest.CreateDefault(new Uri(BuildUrl("/models/AggRootModel/Repositories")));
			// var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri("http://localhost:2740/models/AggRootModel/Repositories"));
			req.Accept = "application/atom+xml";
			req.ContentType = "application/atom+xml";
			req.Method = "POST";
			var reqWriter = new StreamWriter(req.GetRequestStream());
			// todo: write Repository in atom

			reqWriter.Write(
@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
  <entry xml:base=""http://localhost:2740/models/AggRootModel/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">
	<id>http://localhost:2740/models/AggRootModel/Repositories(1)</id>
	<title type=""text""></title>
	<updated>2012-04-20T06:29:23Z</updated>
	<author>
	  <name />
	</author>
	<category term=""ns.Repository"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
	<content type=""application/xml"">
	  <m:properties>
		<d:Name>repo1</d:Name>
	  </m:properties>
	</content>
  </entry>");
			reqWriter.Flush();

			var reply = (HttpWebResponse)req.GetResponse();
			reply.StatusCode.Should().Be(HttpStatusCode.Created);
			reply.ContentType.Should().Be("application/atom+xml; charset=utf-8");
			var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();

			Console.WriteLine(replyContent);

		}
	}
}
