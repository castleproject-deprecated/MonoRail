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
	using System.Collections.Generic;
	using NUnit.Framework;
	using Stubs;

	[TestFixture]
	public class RouteGenerateTests
	{
		[Test]
		public void LiteralRoute_WhenGenerating_OutputsLiteral()
		{
			const string pattern = "/home";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home",
				route.Generate("", new Dictionary<string, string>() { }));
		}

		[Test]
		public void LiteralRoute_WhenGeneratingWithVPath_OutputsLiteralWithVPath()
		{
			const string pattern = "/home";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/app/home",
				route.Generate("/app", new Dictionary<string, string>() { }));
		}
		   
		[Test, ExpectedException(typeof(RouteException), ExpectedMessage = "Missing required parameter for route generation: 'controller'")]
		public void OptAndNamedParam_WhenGenerating_DemandsParameters()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("", 
				route.Generate("", new Dictionary<string, string>() { }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameter()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_1()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home.xml",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" }, { "format", "xml" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_2()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home/index",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_3()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home/index/1",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_4()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home/index/1.json",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" }, { "format", "json" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			route.DefaultValues.Add("action", "index");
			Assert.AreEqual("/home",
				route.Generate("",
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault_2()
		{
			const string pattern = "(/:controller(/:action(/:id)))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			route.DefaultValues.Add("controller", "home");
			route.DefaultValues.Add("action", "index");
			Assert.AreEqual("/",
				route.Generate("/",
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_ForcesDefaultWhenOptionalIsPresent()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			route.DefaultValues.Add("action", "index");
			Assert.AreEqual("/home/index/1",
				route.Generate("", 
				new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
		}

		[Test]
		public void RouteWithNestedRoute_WhenGenerating_GeneratesTheCorrectUrl()
		{
			const string path = "/something";
			var router = new Router();
			router.Match(path, "some", c =>
				c.Match("/else", "else", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("some.else");

			Assert.AreEqual("/something/else",
				route.Generate("", new Dictionary<string, string>() ));
		}

		[Test]
		public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForBase()
		{
			const string path = "/areaname";
			var router = new Router();
			router.Match(path, "area", c =>
				c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("area.default");

			Assert.AreEqual("/areaname", route.Generate("", new Dictionary<string, string>()));
		}

		[Test]
		public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForController()
		{
			const string path = "/areaname";
			var router = new Router();
			router.Match(path, "area", c =>
				c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("area.default");

			Assert.AreEqual("/areaname/home", 
				route.Generate("", 
					new Dictionary<string, string>() { { "controller", "home" } }));
		}

		[Test]
		public void RouteWithTypicalPatternInNestedRoute_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction()
		{
			const string path = "/areaname";
			var router = new Router();
			router.Match(path, "area", c =>
				c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("area.default");

			Assert.AreEqual("/areaname/home/index",
				route.Generate("",
					new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
		}

		[Test]
		public void RouteWithTypicalPatternInNestedRouteAndDefaults_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction()
		{
			const string path = "/areaname";
			var router = new Router();
			router.Match(path, "area", c =>
				c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("area.default");
			route.DefaultValues.Add("controller", "home");
			route.DefaultValues.Add("action", "index");

			Assert.AreEqual("/areaname",
				route.Generate("",
					new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
		}

		[Test]
		public void RouteWithTypicalPatternInNestedRouteAndDefaults_WhenGenerating_GeneratesTheCorrectUrlForControllerAndAction_2()
		{
			const string path = "/areaname";
			var router = new Router();
			router.Match(path, "area", c =>
				c.Match("(/:controller(/:action(/:id)))(.:format)", "default", new DummyHandlerMediator()), new DummyHandlerMediator());

			var route = router.GetRoute("area.default");
			route.DefaultValues.Add("controller", "home");
			route.DefaultValues.Add("action", "index");

			Assert.AreEqual("/areaname/home/test",
				route.Generate("",
					new Dictionary<string, string>() { { "controller", "home" }, { "action", "test" } }));
		}

		private static Route GetRoute(string pattern, string name)
		{
			var router = new Router();
			return router.Match(pattern, name, new DummyHandlerMediator());
		}
	}
}
