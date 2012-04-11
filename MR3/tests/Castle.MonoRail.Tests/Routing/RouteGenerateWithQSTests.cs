namespace Castle.MonoRail.Routing.Tests
{
	using System.Collections.Generic;
	using NUnit.Framework;
	using Stubs;

	[TestFixture]
	public class RouteGenerateWithQSTests
	{
		[Test]
		public void LiteralRoute_WhenGenerating_OutputsLiteral()
		{
			const string pattern = "/home";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home?Name=eva&Age=22",
			                route.Generate("", new Dictionary<string, string>() { { "Name", "eva"}, { "Age", "22" } }));
		}

		[Test]
		public void LiteralRoute_WhenGeneratingWithVPath_OutputsLiteralWithVPath()
		{
			const string pattern = "/home";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/app/home?Name=eva&Age=22",
			                route.Generate("/app", new Dictionary<string, string>() { { "Name", "eva" }, { "Age", "22" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_WorksForRequiredParameter()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home?Name=eva&Age=22",
			                route.Generate("",
			                               new Dictionary<string, string>() { { "controller", "home" }, { "Name", "eva" }, { "Age", "22" } }));
		}

		[Test]
		public void OptAndNamedParam_WhenGenerating_IncludesOptionalsAndQS()
		{
			const string pattern = "/:controller(/:action(/:id))(.:format)";
			const string name = "default";
			Route route = GetRoute(pattern, name);
			Assert.AreEqual("/home.js?Name=eva&Age=22",
			                route.Generate("",
			                               new Dictionary<string, string>() { { "controller", "home" }, { "format", "js" }, { "Name", "eva" }, { "Age", "22" } }));
		}

		private static Route GetRoute(string pattern, string name)
		{
			var router = new Router();
			return router.Match(pattern, name, new DummyHandlerMediator());
		}
	}
}