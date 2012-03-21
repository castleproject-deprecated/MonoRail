namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing.Tests.Stubs;
	using NUnit.Framework;

	[TestFixture]
	public class NestedRouteMatchingWithVPathTests
	{
		private Router _router;

		[SetUp]
		public void Init()
		{
			_router = new Router();
		}

		[Test]
		public void NestedLiteralMatching_MatchesExactSameString()
		{
			const string path = "/something";
			_router.Match(path, c => 
								c.Match("/else", new DummyHandlerMediator()), new DummyHandlerMediator());

			Assert.IsNotNull(_router.TryMatch("/app/something/else", "/app"));
		}

		[Test]
		public void NestedLiteralMatching_DoesNotMatchSimilar()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("/else", new DummyHandlerMediator()), new DummyHandlerMediator());

			Assert.IsNull(_router.TryMatch("/something.else"));
		}

		[Test]
		public void LiteralPlusPattern_MatchesLiteralAndAcceptsParam()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("/:controller", new DummyHandlerMediator()), new DummyHandlerMediator());

			var match = _router.TryMatch("/something/home");
			Assert.IsNotNull(match);
			Assert.AreEqual(1, match.RouteParams.Count);
			Assert.AreEqual("home", match.RouteParams["controller"]);
		}

		[Test]
		public void LiteralPlusOptional_MatchesLiteralAndMatchingParam()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("(/:controller(/:action))", new DummyHandlerMediator()), new DummyHandlerMediator());

			var match = _router.TryMatch("/something/home/index");
			Assert.IsNotNull(match);
			Assert.AreEqual(2, match.RouteParams.Count);
			Assert.AreEqual("home", match.RouteParams["controller"]);
			Assert.AreEqual("index", match.RouteParams["action"]);
		}

		[Test]
		public void LiteralPlusOptional_MatchesLiteral()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("(/:controller(/:action))", new DummyHandlerMediator()), new DummyHandlerMediator());

			var match = _router.TryMatch("/something");
			Assert.IsNotNull(match);
			Assert.AreEqual(0, match.RouteParams.Count);
		}

		[Test]
		public void LiteralPlusOptional_MatchesLiteralAndReturnsDefaults()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("(/:controller(/:action))", 
										dc => 
										dc.Defaults(d => d.Controller("poco").Action("in")), new DummyHandlerMediator()), new DummyHandlerMediator());

			var match = _router.TryMatch("/something");
			Assert.IsNotNull(match);
			Assert.AreEqual(2, match.RouteParams.Count);
			Assert.AreEqual("poco", match.RouteParams["controller"]);
			Assert.AreEqual("in", match.RouteParams["action"]);
		}


		[Test]
		public void LiteralPlusOptional_MatchesLiteralAndReturnsDefaults1()
		{
			const string path = "/viewcomponents";
			_router.Match(path, c =>
								c.Match("(/:controller(/:action))",
										dc =>
										dc.Defaults(d => d.Controller("poco").Action("in")), new DummyHandlerMediator()), new DummyHandlerMediator());

			var match = _router.TryMatch("/viewcomponents/");
			Assert.IsNotNull(match);
			Assert.AreEqual(2, match.RouteParams.Count);
			Assert.AreEqual("poco", match.RouteParams["controller"]);
			Assert.AreEqual("in", match.RouteParams["action"]);
		}

		[Test]
		public void ParentToNestedValuePropagation()
		{
			_router.Match("/viewcomponents", "viewcomponents",
						  r => 
						  r.Invariables(c => c.Area("viewcomponents")).
							  Match("(/:controller(/:action(/:id)))", 
									conf => conf.Defaults(d => d.Action("index"))));

			var match = _router.TryMatch("/viewcomponents/OrderListComponent");

			Assert.IsNotNull(match);
			Assert.AreEqual(3, match.RouteParams.Count);
			Assert.AreEqual("OrderListComponent", match.RouteParams["controller"]);
			Assert.AreEqual("index", match.RouteParams["action"]);
			Assert.AreEqual("viewcomponents", match.RouteParams["area"]);
		}
	}
}