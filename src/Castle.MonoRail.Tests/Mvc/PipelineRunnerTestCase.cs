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
namespace Castle.MonoRail.Tests.Mvc
{
	using System;
	using System.Web;
	using System.Web.Routing;	
	using Moq;
	using NUnit.Framework;
	using MonoRail.Mvc;
	using Primitives.Mvc;

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
							ControllerExecutorProviders = new[] { new Lazy<ControllerExecutorProvider, IOrderMeta>(() => executorProvider.Object, new FakeOrderMeta()) },
							ControllerProviders = new[] { new Lazy<ControllerProvider, IOrderMeta>(() => controllerProvider.Object, new FakeOrderMeta()) }
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

	public class FakeOrderMeta : IOrderMeta
	{
		public int Order
		{
			get { return 1; }
		}
	}
}