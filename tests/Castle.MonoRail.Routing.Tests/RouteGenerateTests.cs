namespace Castle.MonoRail.Routing.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Stubs;

    [TestClass]
    public class RouteGenerateTests
    {
        [TestMethod]
        public void LiteralRoute_WhenGenerating_OutputsLiteral()
        {
            const string pattern = "/home";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home",
                route.Generate("", new Dictionary<string, string>() { }));
        }
           
        [TestMethod, ExpectedException(typeof(RouteException), "Missing required parameter for route generation: 'controller'")]
        public void OptAndNamedParam_WhenGenerating_DemandsParameters()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("", 
                route.Generate("", new Dictionary<string, string>() { }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameter()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_1()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home.xml",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "format", "xml" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_2()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_3()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index/1",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameterAndUsesOptional_4()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            Assert.AreEqual("/home/index/1.json",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" }, { "format", "json" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/home",
                route.Generate("",
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_IgnoresParametersWhenTheyMatchTheDefault_2()
        {
            const string pattern = "(/:controller(/:action(/:id)))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("controller", "home");
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/",
                route.Generate("/",
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } }));
        }

        [TestMethod]
        public void OptAndNamedParam_WhenGenerating_ForcesDefaultWhenOptionalIsPresent()
        {
            const string pattern = "/:controller(/:action(/:id))(.:format)";
            const string name = "default";
            Route route = GetRoute(pattern, name);
            route.RouteConfig.DefaultValueForNamedParam("action", "index");
            Assert.AreEqual("/home/index/1",
                route.Generate("", 
                new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" }, { "id", "1" } }));
        }


        private static Route GetRoute(string pattern, string name)
        {
            var router = new Router();
            return router.Match(pattern, name, new DummyHandlerMediator());
        }
    }
}
