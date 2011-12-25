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
    using System;
    using Castle.MonoRail.Routing.Tests.Stubs;
    using Castle.MonoRail.Routing;
    using NUnit.Framework;

    [TestFixture]
    public class RouteParsingAndConfigTests
    {
        private Router _router;

        [SetUp]
        public void Init()
        {
            _router = new Router();
        }

        [Test]
        public void DefiningRoute_SimpleLiteral_1_Node()
        {
            const string path = "/something";
            var route = _router.Match(path, new DummyHandlerMediator());
            
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);
            var nodes = route.RouteNodes;
            Assert.AreEqual(1, nodes.Length);
            Assert.IsTrue(nodes[0].IsLiteral);
            Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
        }

        [Test]
        public void DefiningRoute_SimpleLiteral_2_Nodes()
        {
            const string path = "/something/else";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(2, nodes.Length);
            Assert.IsTrue(nodes[0].IsLiteral);
            Assert.IsTrue(nodes[1].IsLiteral);
            Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
            Assert.AreEqual("/else", (nodes[1] as Internal.Term.Literal).Item);
        }

        [Test]
        public void DefiningRoute_SimpleLiteralWithDot_2_Nodes()
        {
            const string path = "/something.else";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(2, nodes.Length);
            Assert.IsTrue(nodes[0].IsLiteral);
            Assert.IsTrue(nodes[1].IsLiteral);
            Assert.AreEqual("/something", (nodes[0] as Internal.Term.Literal).Item);
            Assert.AreEqual(".else", (nodes[1] as Internal.Term.Literal).Item);
        }

        [Test]
        public void DefiningRoute_SimpleParam_1_Node()
        {
            const string path = "/:controller";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(1, nodes.Length);
            Assert.IsTrue(nodes[0].IsNamedParam);
            Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
            Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
        }

        [Test]
        public void DefiningRoute_OptionalParam_1_Node()
        {
            const string path = "(/:controller)";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(1, nodes.Length);
            Assert.IsTrue(nodes[0].IsOptional);

            nodes = (nodes[0] as Internal.Term.Optional).Item;

            Assert.IsTrue(nodes[0].IsNamedParam);
            Assert.AreEqual("/", (nodes[0] as Internal.Term.NamedParam).Item1);
            Assert.AreEqual("controller", (nodes[0] as Internal.Term.NamedParam).Item2);
        }

        [Test]
        public void DefiningRoute_OptionalParams_2_Nodes()
        {
            const string path = "/:controller(/:action)";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

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

        [Test]
        public void DefiningRoute_OptionalParams_3_Nodes()
        {
            const string path = "/:controller(/:action(/:id))(.:format)";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

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

        [Test]
        public void DefiningRoute_AllOptional()
        {
            const string path = "(/:controller(/:action))";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(1, nodes.Length);
            Assert.IsTrue(nodes[0].IsOptional);
        }

        [Test]
        public void DefiningRoute_AllOptional_3()
        {
            const string path = "(/:controller(/:action(/:id)))";
            var route = _router.Match(path, new DummyHandlerMediator());
            Assert.IsNotNull(route);
            Assert.AreEqual(path, route.Path);
            Assert.IsNull(route.Name);

            var nodes = route.RouteNodes;
            Assert.AreEqual(1, nodes.Length);
            Assert.IsTrue(nodes[0].IsOptional);
        }
    }
}
