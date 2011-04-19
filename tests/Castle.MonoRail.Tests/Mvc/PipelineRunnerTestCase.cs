#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.MonoRail.Tests.Mvc
{
	using System;
	using System.Web;
	using Castle.MonoRail.Extensibility;
	using Castle.MonoRail.Hosting.Mvc;
	using Castle.MonoRail.Routing;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class PipelineRunnerTestCase
	{
		private PipelineRunner runner;

		private Mock<ControllerExecutorProvider> executorProvider;
		private Mock<ControllerProvider> controllerProvider;
		private Mock<HttpContextBase> context;
		private Mock<ControllerExecutor> executor;
		private RouteMatch routeMatch;
		private ControllerPrototype prototype;

		[SetUp]
		public void Init()
		{
			executorProvider = new Mock<ControllerExecutorProvider>();
			executor = new Mock<ControllerExecutor>();
			controllerProvider = new Mock<ControllerProvider>();
			context = new Mock<HttpContextBase>();

			routeMatch = new RouteMatch();
			prototype = new ControllerPrototype(new object());

			runner = new PipelineRunner
			         	{
							ControllerProviders = new[] { new Lazy<ControllerProvider, IComponentOrder>(() => controllerProvider.Object, new FakeOrderMeta()) },
							ControllerExecutorProviders = new[] { new Lazy<ControllerExecutorProvider, IComponentOrder>(() => executorProvider.Object, new FakeOrderMeta()) }
			         	};
		}

		[Test]
		public void Process_should_find_the_controller_prototype_by_inquiring_controller_providers()
		{
			controllerProvider.Setup(cp => cp.Create(routeMatch, context.Object)).Returns(prototype);

			executorProvider.Setup(ep => ep.Create(prototype, routeMatch, context.Object)).Returns(executor.Object);

			runner.Execute(routeMatch, context.Object);

			controllerProvider.VerifyAll();
		}

		[Test]
		public void Process_should_find_and_invoke_the_controller_executor()
		{
			controllerProvider.Setup(cp => cp.Create(routeMatch, context.Object)).Returns(prototype);

			executorProvider.Setup(ep => ep.Create(prototype, routeMatch, context.Object)).Returns(executor.Object);

			executor.Setup(e => e.Execute(prototype, routeMatch, context.Object));

			runner.Execute(routeMatch, context.Object);

			executor.VerifyAll();
		}
	}

	public class FakeOrderMeta : IComponentOrder
	{
		public int Order
		{
			get { return 1; }
		}
	}
}
