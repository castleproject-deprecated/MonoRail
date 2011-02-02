// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Tests.Services
{
	using System.Reflection;
	using Castle.Core.Logging;
	using Castle.MonoRail.Framework.Services;
	using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture]
	public class DefaultControllerFactoryTestCase
	{
		private DefaultControllerFactory factory;
		private TestServiceContainer container;
		private ILogger logger;

		[TestFixtureSetUp]
		public void Init()
		{
			var loggerFactory = MockRepository.GenerateStub<ILoggerFactory>();
			logger = MockRepository.GenerateStub<ILogger>();
			loggerFactory.Stub(f => f.Create(typeof (AbstractControllerFactory))).IgnoreArguments().Repeat.Any().Return(logger);
			factory = new DefaultControllerFactory();
			container = new TestServiceContainer();
			container.AddService(typeof (ILoggerFactory), loggerFactory);
			factory.Service(container);
			factory.Inspect(Assembly.GetExecutingAssembly());
		}

		[Test]
		public void EmptyArea()
		{
			var controller = factory.CreateController("", "home");

			Assert.IsNotNull(controller);
			Assert.AreEqual("Castle.MonoRail.Framework.Tests.Controllers.HomeController",
			                controller.GetType().FullName);
		}

		[Test]
		public void OneLevelArea()
		{
			var controller = factory.CreateController("clients", "home");

			Assert.IsNotNull(controller);
			Assert.AreEqual("Castle.MonoRail.Framework.Tests.Controllers.Clients.ClientHomeController",
			                controller.GetType().FullName);

			controller = factory.CreateController("clients", "hire-us");

			Assert.IsNotNull(controller);
			Assert.AreEqual("Castle.MonoRail.Framework.Tests.Controllers.Clients.OtherController",
			                controller.GetType().FullName);

			controller = factory.CreateController("ourproducts", "shoppingcart");

			Assert.IsNotNull(controller);
			Assert.AreEqual("Castle.MonoRail.Framework.Tests.Controllers.Products.CartController",
			                controller.GetType().FullName);

			controller = factory.CreateController("ourproducts", "lista");

			Assert.IsNotNull(controller);
			Assert.AreEqual("Castle.MonoRail.Framework.Tests.Controllers.Products.ListController",
			                controller.GetType().FullName);
		}

		[Test]
		public void InitializeControllerLogger()
		{
			var controller = factory.CreateController("", "home");
			Assert.That(((Controller) controller).Logger, Is.EqualTo(logger));
		}
	}
}