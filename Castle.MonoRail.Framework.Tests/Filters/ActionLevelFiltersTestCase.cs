// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


namespace Castle.MonoRail.Framework.Tests.Filters
{
	using NUnit.Framework;
	using Rhino.Mocks;
	using Test;

	[TestFixture]
	public class ActionLevelFiltersTestCase
	{
		private MockRepository mockRepository = new MockRepository();
		private StubEngineContext engineContext;
		private StubViewEngineManager engStubViewEngineManager;
		private StubMonoRailServices services;
		private IFilterFactory filterFactoryMock;

		[SetUp]
		public void Init()
		{
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
		public void Filter_BeforeActionReturningFalsePreventsActionProcessment()
		{
			var controller = new ControllerWithSingleBeforeActionFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
				Expect.Call(filterFactoryMock.Create(typeof(DummyFilter))).Return(filterMock);

				Expect.Call(filterMock.Perform(ExecuteWhen.BeforeAction, engineContext, controller, context)).Return(false);

				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.IsNull(engStubViewEngineManager.TemplateRendered);
				Assert.IsFalse(controller.indexActionExecuted);
			}
		}

		[Test]
		public void Filter_BeforeActionReturningTrueAllowsProcessToContinue()
		{
			var controller = new ControllerWithSingleBeforeActionFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
				Expect.Call(filterFactoryMock.Create(typeof(DummyFilter))).Return(filterMock);

				Expect.Call(filterMock.Perform(ExecuteWhen.BeforeAction, engineContext, controller, context)).Return(true);

				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.AreEqual("home\\index", engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(controller.indexActionExecuted);
			}
		}

		[Test]
		public void Filter_AfterActionIsRun()
		{
			var controller = new ControllerWithAfterActionFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
				Expect.Call(filterFactoryMock.Create(typeof(DummyFilter))).Return(filterMock);

				Expect.Call(filterMock.Perform(ExecuteWhen.AfterAction, engineContext, controller, context)).Return(true);

				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.AreEqual("home\\index", engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(controller.indexActionExecuted);
			}
		}

		[Test]
		public void Filter_AfterActionReturningFalsePreventsRendering()
		{
			var controller = new ControllerWithAfterActionFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
				Expect.Call(filterFactoryMock.Create(typeof(DummyFilter))).Return(filterMock);

				Expect.Call(filterMock.Perform(ExecuteWhen.AfterAction, engineContext, controller, context)).Return(false);

				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.IsNull(engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(controller.indexActionExecuted);
			}
		}

		[Test]
		public void Filter_AfterRenderingIsRun()
		{
			var controller = new ControllerWithAfterRenderingFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
				Expect.Call(filterFactoryMock.Create(typeof(DummyFilter))).Return(filterMock);

				Expect.Call(filterMock.Perform(ExecuteWhen.AfterRendering, engineContext, controller, context)).Return(true);

				filterFactoryMock.Release(filterMock);
				LastCall.Repeat.Once();
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.AreEqual("home\\index", engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(controller.indexActionExecuted);
			}
		}
		/*
		[Test]
		public void Filter_SkipFilterAttributeSkipsTheFilter()
		{
			var controller = new ControllerWithSkipFilter();

			var context = services.ControllerContextFactory.
				Create("", "home", "index",
				       services.ControllerDescriptorProvider.BuildDescriptor(controller));

			var filterMock = mockRepository.DynamicMock<IFilter>();

			using(mockRepository.Record())
			{
			}

			using(mockRepository.Playback())
			{
				controller.Process(engineContext, context);
				controller.Dispose();

				Assert.AreEqual("home\\index", engStubViewEngineManager.TemplateRendered);
				Assert.IsTrue(controller.indexActionExecuted);
			}
		}
		*/
		#region Controllers

		private class ControllerWithSingleBeforeActionFilter : Controller
		{
			public bool indexActionExecuted;
			[Filter(ExecuteWhen.BeforeAction, typeof(DummyFilter))]
			public void Index()
			{
				indexActionExecuted = true;
			}
		}
		/*
		[Filter(ExecuteWhen.BeforeAction, typeof(DummyFilter))]
		private class ControllerWithSkipFilter : Controller
		{
			public bool indexActionExecuted;

			[SkipFilter]
			public void Index()
			{
				indexActionExecuted = true;
			}
		}*/

		private class ControllerWithAfterActionFilter : Controller
		{
			public bool indexActionExecuted;
			[Filter(ExecuteWhen.AfterAction, typeof(DummyFilter))]
			public void Index()
			{
				indexActionExecuted = true;
			}
		}

		private class ControllerWithAfterRenderingFilter : Controller
		{
			public bool indexActionExecuted;
			[Filter(ExecuteWhen.AfterRendering, typeof(DummyFilter))]
			public void Index()
			{
				indexActionExecuted = true;
			}
		}

		#endregion

		private class DummyFilter : IFilter
		{
			public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller,
			                    IControllerContext controllerContext)
			{
				return false;
			}
		}
	}
}