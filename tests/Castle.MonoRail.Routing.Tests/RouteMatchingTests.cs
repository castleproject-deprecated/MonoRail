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

namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class RouteMatchingTests
	{
		[TestMethod]
		public void LiteralMatching_MatchesExactSameString()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());
			
			Assert.IsNull(router.TryMatch("/else"));
			var data = router.TryMatch(path);
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_MatchesExactSameString_IgnoringCase()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/SomEThinG");
			Assert.IsNotNull(data);
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerString()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			Assert.IsNull(router.TryMatch("/somethingelse"));
		}

		[TestMethod]
		public void LiteralMatching_DoesNot_MatchesSimilarButLongerComposablePath()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			Assert.IsNull(router.TryMatch("/something/else"));
			Assert.IsNull(router.TryMatch("/something.else"));
		}

		[TestMethod]
		public void NamedParamMatching_DoesNotMatch_EmptyInput()
		{
			const string path = "/:something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

		[TestMethod]
		public void NamedParamMatching_Matches_And_ExposesParamValue()
		{
			const string path = "/:something";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

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
			router.Match(path, new DummyHandlerMediator());

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
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/testing");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
		}

		[TestMethod]
		public void NamedParamMatching_And_OptionalMatching_DoesNotMatch_Empty()
		{
			const string path = "/:controller(/:action)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase()
		{
			const string path = "(/:action)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

        [TestMethod]
        public void OptionalMatching_WithDefault_ReturnsDefaultWhenEmpty()
        {
            const string path = "(/:action)";
            var router = new Router();
            router.Match(path, r => r.Defaults(d => d.Action("test")), new DummyHandlerMediator());

            var data = router.TryMatch("/");
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.RouteParams.Count);
            Assert.AreEqual("test", data.RouteParams["action"]);
        }

		[TestMethod]
		public void OptionalMatching_Matches_SimpleCase_Empty()
		{
			const string path = "(/:action)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

		[TestMethod]
		public void OptionalMatching_Matches_NamedParam_And_Optional()
		{
			const string path = "/:controller(/:action)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/testing/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("testing", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_Empty()
		{
			const string path = "(/:controller(/:action))";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

        [TestMethod]
        public void OptionalMatching_AllOptionals_WithDefaults_ReturnDefaultsWhenEmpty()
        {
            const string path = "(/:controller(/:action))";
            var router = new Router();
            router.Match(path, c => c.Defaults(d => d.Controller("home").Action("test")), new DummyHandlerMediator());

            var data = router.TryMatch("/");
            Assert.AreEqual(2, data.RouteParams.Count);
            Assert.AreEqual("home", data.RouteParams["controller"]);
            Assert.AreEqual("test", data.RouteParams["action"]);
        }

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_First()
		{
			const string path = "(/:controller(/:action))";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/home");
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_Matches_All()
		{
			const string path = "(/:controller(/:action))";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/home/index");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Empty()
		{
			const string path = "(/:controller(/:action))(.:format)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/");
			Assert.IsNotNull(data);
			Assert.AreEqual(0, data.RouteParams.Count);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation1()
		{
			const string path = "(/:controller(/:action))(.:format)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/home.xml");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("xml", data.RouteParams["format"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation2()
		{
			const string path = "(/:controller(/:action))(.:format)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/home/index.xml");
			Assert.IsNotNull(data);
			Assert.AreEqual(3, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
			Assert.AreEqual("xml", data.RouteParams["format"]);
		}

		[TestMethod]
		public void OptionalMatching_AllOptionals_NotNested_Matches_Variation3()
		{
			const string path = "(/:controller(/:action))(.:format)";
			var router = new Router();
			router.Match(path, new DummyHandlerMediator());

			var data = router.TryMatch("/home/index.");
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.RouteParams.Count);
			Assert.AreEqual("home", data.RouteParams["controller"]);
			Assert.AreEqual("index", data.RouteParams["action"]);
		}
	}
}
