namespace Castle.MonoRail.Routing.Tests
{
	using System;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Castle.MonoRail.Routing;

	[TestClass]
	public class RouteParsingAndConfigTests
	{
		[TestMethod]
		public void DefiningRoute_SimpleLiteral_1_Node()
		{
			const string path = "/something";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsLiteral);
			Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
		}

		[TestMethod]
		public void DefiningRoute_SimpleLiteral_2_Nodes()
		{
			const string path = "/something/else";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(2, nodes.Length);
			Assert.IsTrue(nodes[0].IsLiteral);
			Assert.IsTrue(nodes[1].IsLiteral);
			Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
			Assert.AreEqual("/else", (nodes[1] as Internal.Term.Literal).Item);
		}

		[TestMethod]
		public void DefiningRoute_SimpleLiteralWithDot_2_Nodes()
		{
			const string path = "/something.else";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(2, nodes.Length);
			Assert.IsTrue(nodes[0].IsLiteral);
			Assert.IsTrue(nodes[1].IsLiteral);
			Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
			Assert.AreEqual(".else", (nodes[1] as Internal.Term.Literal).Item);
		}

		[TestMethod]
		public void DefiningRoute_SimpleParam_1_Node()
		{
			const string path = "/:controller";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
		}

		[TestMethod]
		public void DefiningRoute_OptionalParam_1_Node()
		{
			const string path = "(/:controller)";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsOptional);

			nodes = (nodes[0] as Internal.Term.Optional).Item;

			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
		}

		[TestMethod]
		public void DefiningRoute_OptionalParams_2_Nodes()
		{
			const string path = "/:controller(/:action)";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(2, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);

			Assert.IsTrue(nodes[1].IsOptional);
			nodes = (nodes[1] as Internal.Term.Optional).Item;

			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("action", (nodes[0] as Internal.Term.NamedParam).Item2);
		}

		[TestMethod]
		public void DefiningRoute_OptionalParams_3_Nodes()
		{
			const string path = "/:controller(/:action(/:id))(.:format)";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(3, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.IsTrue(nodes[1].IsOptional);
			Assert.IsTrue(nodes[2].IsOptional);

			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);

			nodes = (nodes[1] as Internal.Term.Optional).Item;

			Assert.AreEqual(2, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("action", (nodes[0] as Internal.Term.NamedParam).Item2);

			Assert.IsTrue(nodes[1].IsOptional);
			Assert.AreEqual(1, (nodes[1] as Internal.Term.Optional).Item.Length);
			Assert.IsTrue((nodes[1] as Internal.Term.Optional).Item[0].IsNamedParam);

			nodes = route.RouteNodes;
			nodes = (nodes[2] as Internal.Term.Optional).Item;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsNamedParam);
			Assert.AreEqual(".", (nodes[0] as Internal.Term.NamedParam).Item1);
			Assert.AreEqual("format", (nodes[0] as Internal.Term.NamedParam).Item2);
		}

		[TestMethod]
		public void DefiningRoute_AllOptional()
		{
			const string path = "(/:controller(/:action))";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsOptional);
//			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
//			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
//
//			Assert.IsTrue(nodes[1].IsOptional);
//			nodes = (nodes[1] as Internal.Term.Optional).Item;
//
//			Assert.AreEqual(1, nodes.Length);
//			Assert.IsTrue(nodes[0].IsNamedParam);
//			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
//			Assert.AreEqual("action", (nodes[0] as Internal.Term.NamedParam).Item2);
		}

		[TestMethod]
		public void DefiningRoute_AllOptional_3()
		{
			const string path = "(/:controller(/:action(/:id)))";
			var router = new Router();
			var route = router.Match(path, new DummyHandlerMediator());
			Assert.IsNotNull(route);
			Assert.AreEqual(path, route.Path);
			Assert.IsNull(route.Name);
			Assert.IsNull(route.RouteConfig);

			var nodes = route.RouteNodes;
			Assert.AreEqual(1, nodes.Length);
			Assert.IsTrue(nodes[0].IsOptional);
//			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
//			Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
//
//			Assert.IsTrue(nodes[1].IsOptional);
//			nodes = (nodes[1] as Internal.Term.Optional).Item;
//
//			Assert.AreEqual(1, nodes.Length);
//			Assert.IsTrue(nodes[0].IsNamedParam);
//			Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
//			Assert.AreEqual("action", (nodes[0] as Internal.Term.NamedParam).Item2);
		}
	}
}
