namespace Castle.MonoRail.Tests.Routing
{
	using System;
	using FluentAssertions;
	using MonoRail.Routing;
	using NUnit.Framework;

	[TestFixture]
	public class RequestInfoTestCase
	{
		[Test]
		public void Http_ImplicitDefaultPort_NoVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("http://localhost/"), "");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("http://localhost/");
		}

		[Test]
		public void Http_ExplicitDefaultPort_NoVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("http://localhost:80/"), "");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("http://localhost/");
		}

		[Test]
		public void Https_ImplicitDefaultPort_NoVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("https://localhost/"), "");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("https://localhost/");
		}

		[Test]
		public void Https_ExplicitDefaultPort_NoVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("https://localhost:443/"), "");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("https://localhost/");
		}

		[Test]
		public void Http_NonEmptyVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("http://localhost:80/"), "/app");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("http://localhost/app");
		}

		[Test]
		public void Http_LongVPath()
		{
			var ri = new RequestInfo("/some/path", new Uri("http://localhost:80/"), "/app/something/1.00/else");
			var uri = ri.BaseUri;
			uri.AbsoluteUri.Should().Be("http://localhost/app/something/1.00/else");
		}
	}
}
