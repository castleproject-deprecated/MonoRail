namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class RouteMatchinTests
	{
		[TestMethod]
		public void LiteralMatching_MatchesExactSameString()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path);
			
			Assert.IsNull(router.TryMatch("/else"));
			var data = router.TryMatch(path);
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_MatchesExactSameString_IgnoringCase()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/SomEThinG");
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerString()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path);

			Assert.IsNull(router.TryMatch("/somethingelse"));
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerComposablePath()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path);

			Assert.IsNull(router.TryMatch("/something/else"));
			Assert.IsNull(router.TryMatch("/something.else"));
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue()
		{
			const string path = "/:something";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/something");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("something", data.RouteParams["something"]);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue2()
		{
			const string path = "/:something";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/testing");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["something"]);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue3()
		{
			const string path = "/:controller(/:action)";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/testing");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
		}

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase()
		{
			const string path = "(/:action)";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase_Empty()
		{
			const string path = "(/:action)";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_Matches_NamedParam_And_Optional()
		{
			const string path = "/:controller(/:action)";
			var router = new Router();
			router.Match(path);

			var data = router.TryMatch("/testing/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}
	}
}
