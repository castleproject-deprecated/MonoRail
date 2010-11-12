namespace Castle.MonoRail3.Tests
{
	using System.Web;
	using Moq;
	using NUnit.Framework;
	using Primitives.Mvc;

	[TestFixture]
	public class StringResultTestCase
	{
		[Test]
		public void Execute_should_write_on_response_output_stream()
		{
			var http = new Mock<HttpContextBase>();
			var response = new Mock<HttpResponseBase>();
			var context = new ActionResultContext("test", "testcontroller", "action", http.Object);

			var result = new StringResult("value");

			http.SetupGet(ctx => ctx.Response).Returns(response.Object);

			response.Setup(rp => rp.Write("value"));

			result.Execute(context, null);
		}
	}
}
