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
            reply.ContentType.Should().Be("application/xml; charset=utf-8");
            var replyContent = new StreamReader(reply.GetResponseStream()).ReadToEnd();
            replyContent.Should().BeEquivalentTo(@"{
	""d"": [
		{
			""__metadata"": {
				""uri"": ""http://localhost:1302/models/Repo/Repositories(1)"",
				""type"": ""ns.Repository""
			},
			""Id"": 1,
			""Name"": ""repo1"",
			""Branches"": {
				""__deferred"": {
					""uri"": ""http://localhost:1302/models/Repo/Repositories(1)/Branches""
				}
			}
		}
	]
}");
        }
    }
}
