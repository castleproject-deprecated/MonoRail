#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
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
