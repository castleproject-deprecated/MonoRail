namespace Castle.MonoRail.Integration.Tests
{
	using System;
	using System.IO;
	using System.Net;
	using FluentAssertions;
	using NUnit.Framework;
	using WebSiteForIntegration.Controllers;

	[TestFixture, Category("Integration")]
	public class ActionResultsIntegrationTestCase : BaseServerTest
	{
		[Test]
		public void OutputWriterResult_WritesBack()
		{
			var controller = new RootController();
			controller.Index().Should().BeOfType<OutputWriterResult>();

			var req = WebRequest.CreateDefault(new Uri("http://localhost:1302/"));
			var reply = (HttpWebResponse) req.GetResponse();

			reply.StatusCode.Should().Be(HttpStatusCode.OK);
			reply.ContentType.Should().Be("text/html");
			new StreamReader(reply.GetResponseStream()).ReadToEnd().Should().Be("Howdy");
		}

	}
}
