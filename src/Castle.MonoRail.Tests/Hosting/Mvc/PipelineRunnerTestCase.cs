namespace Castle.MonoRail.Tests.Hosting.Mvc
{
	using System.Web;
	using System.Web.Routing;	
	using Moq;
	using NUnit.Framework;
	using Castle.MonoRail.Hosting.Mvc;
	using Castle.MonoRail.Primitives.Mvc;
	using Primitives;

	[TestFixture]
	public class PipelineRunnerTestCase
	{
		private PipelineRunner runner;

		private Mock<ControllerExecutorProvider> executorProvider;
		private Mock<ControllerProvider> controllerProvider;
		private Mock<HttpContextBase> context;
		private Mock<ControllerExecutor> executor;
		private RouteData routeData;
		private ControllerMeta meta;

		[SetUp]
		public void Init()
		{
			executorProvider = new Mock<ControllerExecutorProvider>();
			executor = new Mock<ControllerExecutor>();
			controllerProvider = new Mock<ControllerProvider>();
			context = new Mock<HttpContextBase>();

			routeData = new RouteData();
			meta = new ControllerMeta(new object());

			runner = new PipelineRunner
			         	{
							ControllerExecutorProviders = new[] { executorProvider.Object },
							ControllerProviders = new[] { controllerProvider.Object }
			         	};
		}

		[Test]
		public void Process_should_find_the_controller_meta_inquiring_controller_providers()
		{
			controllerProvider.Setup(cp => cp.Create(routeData)).Returns(meta);

			executorProvider.Setup(ep => ep.CreateExecutor(meta, routeData, context.Object)).Returns(executor.Object);

			runner.Process(routeData, context.Object);

			controllerProvider.VerifyAll();
		}

		[Test]
		public void Process_should_find_and_invoke_the_controller_executor()
		{
			controllerProvider.Setup(cp => cp.Create(routeData)).Returns(meta);

			executorProvider.Setup(ep => ep.CreateExecutor(meta, routeData, context.Object)).Returns(executor.Object);

			executor.Setup(e => e.Process(context.Object));

			runner.Process(routeData, context.Object);

			executor.VerifyAll();
		}
	}
}
