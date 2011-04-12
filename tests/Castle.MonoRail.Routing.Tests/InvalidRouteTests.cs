namespace Castle.MonoRail.Routing.Tests
{
	using System;
	using Castle.MonoRail.Routing;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class InvalidRouteTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg1()
		{
			var router = new Router();
			router.Match(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg2()
		{
			var router = new Router();
			router.Match("");
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_1()
		{
			var router = new Router();
			router.Match("/something.");
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_2()
		{
			var router = new Router();
			router.Match("something");
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_3()
		{
			var router = new Router();
			router.Match("/:controller(/:action");
		}
		
		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_4()
		{
			var router = new Router();
			router.Match("/:controller(/:action)/)");
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_5()
		{
			var router = new Router();
			router.Match("/:controller((/:action)");
		}
	}
}
