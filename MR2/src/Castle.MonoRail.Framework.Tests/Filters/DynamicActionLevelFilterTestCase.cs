

namespace Castle.MonoRail.Framework.Tests.Filters {
	using System;
	using Castle.MonoRail.Framework;
	using Castle.MonoRail.Framework.Test;
	using Castle.MonoRail.Framework.Tests.Actions;
	using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture,Ignore]
	public class DynamicActionLevelFilterTestCase {

		private MockRepository mockRepository = new MockRepository();
		private StubEngineContext engineContext;
		private StubViewEngineManager engStubViewEngineManager;
		private StubMonoRailServices services;
		private IFilterFactory filterFactoryMock;

		[SetUp]
		public void Init() {
			var request = new StubRequest();
			var response = new StubResponse();
			services = new StubMonoRailServices();
			engStubViewEngineManager = new StubViewEngineManager();
			services.ViewEngineManager = engStubViewEngineManager;
			filterFactoryMock = mockRepository.DynamicMock<IFilterFactory>();
			services.FilterFactory = filterFactoryMock;
			engineContext = new StubEngineContext(request, response, services, null);
		}

		[Test]
		public void FilterThrowingExceptionIsTrigged() {
			var action = new ThrowsExceptionFilterDecoratedAction();
			var controller = new DynamicActionExecutorTestCase.DummyController();


			var controllerContext = services.ControllerContextFactory.Create("area", "controller", "action", services.ControllerDescriptorProvider.BuildDescriptor(controller));
			var thrownexception = new Exception(string.Format("thrown at {0}", DateTime.Now));
			controllerContext.DynamicActions["action"] = action;
			var filterMock = new ThrowsExceptionFilter(() => thrownexception);//mockRepository.DynamicMock<IFilter>();
			
			using (mockRepository.Record()) {
				Expect.Call(filterFactoryMock.Create(typeof(ThrowsExceptionFilter))).Return(filterMock);


				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using (mockRepository.Playback())
			{
				Assert.Throws(
					thrownexception.GetType()
					, () => controller.Process(engineContext, controllerContext)
					, thrownexception.Message
					);
				
				controller.Dispose();
				Console.WriteLine(engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(action.executed);
				Assert.IsTrue(filterMock.executed);
			}

		}
	}


	[Filter(ExecuteWhen.AfterRendering, typeof(ThrowsExceptionFilter))]
	public class ThrowsExceptionFilterDecoratedAction : IDynamicAction
	{
		public bool executed = false;
		public object Execute(IEngineContext engineContext, IController controller, IControllerContext controllerContext)
		{
			executed = true;
			return true;
		}
	}

	public class ThrowsExceptionFilter : IFilter {
		private Func<Exception> exceptionFactory;
		public bool executed = false;

		public ThrowsExceptionFilter() : this(() => new Exception())
		{
		}

		public ThrowsExceptionFilter(Func<Exception> exceptionFactory) {
			this.exceptionFactory = exceptionFactory;
		}

		public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller, IControllerContext controllerContext) {
			try
			{
				throw exceptionFactory();
			}
			finally
			{
				executed = true;
			}
		}

	}
}