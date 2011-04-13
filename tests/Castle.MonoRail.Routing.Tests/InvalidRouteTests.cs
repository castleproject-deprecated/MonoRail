namespace Castle.MonoRail.Routing.Tests
{
	using System;
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class InvalidRouteTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg1()
		{
			var router = new Router();
			router.Match(null, new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg2()
		{
			var router = new Router();
			router.Match("", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_1()
		{
			var router = new Router();
			router.Match("/something.", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_2()
		{
			var router = new Router();
			router.Match("something", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_3()
		{
			var router = new Router();
			router.Match("/:controller(/:action", new DummyHandlerMediator());
		}
		
		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_4()
		{
			var router = new Router();
			router.Match("/:controller(/:action)/)", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_5()
		{
			var router = new Router();
			router.Match("/:controller((/:action)", new DummyHandlerMediator());
		}
	}
}
