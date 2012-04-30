namespace Castle.MonoRail.Tests
{
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ContentNegotiatorTestCase
	{
		/**
        Accept: text/*, text/html, text/html;level=1, * / *

		have the following precedence:

        1) text/html;level=1
        2) text/html
        3) text/*
        4) * / *
		  
		 */
		[Test]
		public void Qualities()
		{
			var negotiator = new ContentNegotiator();
			var mime = negotiator.ResolveBestContentType(
						accept: new[] { "text/*", "text/html", "text/html;level=1", "*/*" },
						supports: new[] { "text/html" });

			// mime.Should().Be("application/rrddff");
		}

		/**
        Accept: text/*;q=0.3, text/html;q=0.7, text/html;level=1,
                text/html;level=2;q=0.4, * / *;q=0.5

		would cause the following values to be associated:

        text/html;level=1         = 1
        text/html                 = 0.7
        text/plain                = 0.3

        image/jpeg                = 0.5
        text/html;level=2         = 0.4
        text/html;level=3         = 0.7
		 */
		[Test]
		public void Qualities2()
		{
			var negotiator = new ContentNegotiator();
			var mime = negotiator.ResolveContentType("application/rrddff");
			mime.Should().Be("application/rrddff");
		}



		[Test]
		public void CustomMediaTypeIsResolvedToItself()
		{
			var negotiator = new ContentNegotiator();
			var mime = negotiator.ResolveContentType("application/rrddff");
			mime.Should().Be("application/rrddff");
		}

		[Test]
		public void TypicalAppMediaTypes()
		{
			var negotiator = new ContentNegotiator();
			negotiator.ResolveContentType("application/json").Should().Be(MediaTypes.JSon);
			negotiator.ResolveContentType("application/xml").Should().Be(MediaTypes.Xml);
			negotiator.ResolveContentType("application/atom+xml").Should().Be(MediaTypes.Atom);
			negotiator.ResolveContentType("application/rss+xml").Should().Be(MediaTypes.Rss);
			negotiator.ResolveContentType("application/soap+xml").Should().Be(MediaTypes.Soap);
			negotiator.ResolveContentType("application/xhtml+xml").Should().Be(MediaTypes.XHtml);
			negotiator.ResolveContentType("application/x-www-form-urlencoded").Should().Be(MediaTypes.FormUrlEncoded);
			negotiator.ResolveContentType("application/javascript").Should().Be(MediaTypes.Js);
			negotiator.ResolveContentType("application/js").Should().Be(MediaTypes.Js);
		}

		[Test]
		public void OtherMediaTypes()
		{
			var negotiator = new ContentNegotiator();
			negotiator.ResolveContentType("multipart/form-data; boundary=----------------------------952e70d7bb94").Should().Be(MediaTypes.FormUrlEncoded);
			negotiator.ResolveContentType("multipart/form-data").Should().Be(MediaTypes.FormUrlEncoded);
			negotiator.ResolveContentType("text/javascript").Should().Be(MediaTypes.Js);
			negotiator.ResolveContentType("text/html").Should().Be(MediaTypes.Html);

		}
	}
}
