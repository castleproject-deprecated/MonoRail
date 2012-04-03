#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class RouteMatchingTests
	{
		[Test]
		public void NestedRouteWithGreedyTerm_CanMatch()
		{
			_router.Match("/something",
				config =>
				{
					config.Invariables(dc => dc.Controller("Something"));
					config.Match("/$metadata", rc => rc.Invariables(dc => dc.Action("Metadata")));
					config.Match("/**");
				});

			var data = _router.TryMatch("/something/$metadata");
			Assert.IsNotNull(data);
			data.RouteParams["controller"].Should().Be("Something");
			data.RouteParams["action"].Should().Be("Metadata");

			data = _router.TryMatch("/something/Products(0)");
			Assert.IsNotNull(data);
			data.RouteParams.Should().Contain("controller", "Something");
			data.RouteParams.Should().NotContainKey("action");
			data.RouteParams["GreedyMatch"].Should().Be("/Products(0)");
		}

		[Test]
		public void NestedLiteralMatching_MatchesExactSameString()
		{
			const string path = "/something";
			_router.Match(path, c =>
								c.Match("/else", new DummyHandlerMediator()), new DummyHandlerMediator());

			Assert.IsNotNull(_router.TryMatch("/something/else"));
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
			_router.Match(
				path: "/viewcomponents",
				name: "vc",
				config: r => r.
						Invariables(c => c.Area("viewcomponents")).
						Match("(/:controller(/:action(/:id)))",
							conf => conf.Defaults(d => d.Action("index"))));

			var match = _router.TryMatch("/viewcomponents/OrderListComponent");

			Assert.IsNotNull(match);
			Assert.AreEqual(3, match.RouteParams.Count);
			Assert.AreEqual("viewcomponents", match.RouteParams["area"]);
			Assert.AreEqual("OrderListComponent", match.RouteParams["controller"]);
			Assert.AreEqual("index", match.RouteParams["action"]);
		}
	}
}
