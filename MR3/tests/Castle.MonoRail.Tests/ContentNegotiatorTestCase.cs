namespace Castle.MonoRail.Tests
{
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ContentNegotiatorTestCase
	{
		[Test, ExpectedException(ExpectedMessage = "Unknown format in content-type")]
		public void InvalidContentTypes()
		{
			var negotiator = new ContentNegotiator();
			negotiator.ResolveContentType("application/rrddff");
		}

		[Test]
		public void TypicalAppMimeTypes()
		{
			var negotiator = new ContentNegotiator();
			negotiator.ResolveContentType("application/json").Should().Be(MimeType.JSon);
			negotiator.ResolveContentType("application/xml").Should().Be(MimeType.Xml);
			negotiator.ResolveContentType("application/atom+xml").Should().Be(MimeType.Atom);
			negotiator.ResolveContentType("application/rss+xml").Should().Be(MimeType.Rss);
			// negotiator.ResolveContentType("application/soap+xml").Should().Be(MimeType.);
			negotiator.ResolveContentType("application/xhtml+xml").Should().Be(MimeType.Html);
			negotiator.ResolveContentType("application/x-www-form-urlencoded").Should().Be(MimeType.FormUrlEncoded);
			negotiator.ResolveContentType("application/javascript").Should().Be(MimeType.Js);
			negotiator.ResolveContentType("application/js").Should().Be(MimeType.Js);
		}

		[Test]
		public void OtherMimeTypes()
		{
			var negotiator = new ContentNegotiator();
			negotiator.ResolveContentType("multipart/form-data; boundary=----------------------------952e70d7bb94").Should().Be(MimeType.FormUrlEncoded);
			negotiator.ResolveContentType("multipart/form-data").Should().Be(MimeType.FormUrlEncoded);
			negotiator.ResolveContentType("text/javascript").Should().Be(MimeType.Js);
			negotiator.ResolveContentType("text/html").Should().Be(MimeType.Html);

		}
	}
}
