namespace Castle.MonoRail.Tests
{
	using NUnit.Framework;

	[TestFixture]
	public class AcceptHeaderParserTestCase
	{
		[Test]
		public void StandardHeader()
		{
			var infos = AcceptHeaderParser.parse(new[] {"text/html", "application/xhtml+xml", "*/*"});

			Assert.AreEqual(3, infos.Length);
		}

		[Test]
		public void OfficeHeaders()
		{
			var infos =
				AcceptHeaderParser.parse(new[]
					{
						"application/x-ms-application", "image/jpeg", "application/xaml+xml",
						"image/gif", "image/pjpeg", "application/x-ms-xbap", "application/vnd.ms-excel",
						"application/vnd.ms-powerpoint", "application/msword", "*/*"
					});

			Assert.AreEqual(10, infos.Length);
		}
	}
}