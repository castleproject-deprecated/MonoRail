namespace Castle.MonoRail3.Tests.Hosting.Mvc
{
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail3.Hosting.Mvc;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ComposableMvcHandlerTestCase
	{
		[Test]
		public void ProcessRequest_should_invoke_pipeline_runner()
		{
			var routeData = new RouteData();
			var parser = new Mock<RequestParser>();
			var pipeline = new Mock<PipelineRunner>();
			var context = new Mock<HttpContextBase>();

			var handler = new ComposableMvcHandler{RequestParser = parser.Object, Runner = pipeline.Object};

			parser.Setup(p => p.ParseDescriminators(It.IsAny<HttpRequestBase>())).Returns(routeData);

			pipeline.Setup(p => p.Process(routeData, context.Object));

			handler.ProcessRequest(context.Object);

			pipeline.VerifyAll();
		}
	}
}
