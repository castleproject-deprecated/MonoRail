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
	using System;
	using System.Runtime.InteropServices;
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class InvalidRouteTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg1()
		{
			var router = new Router();
			router.Match(null, new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DefiningRoute_InvalidArg2()
		{
			var router = new Router();
			router.Match("", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_1()
		{
			var router = new Router();
			router.Match("/something.", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_2()
		{
			var router = new Router();
			router.Match("something", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_3()
		{
			var router = new Router();
			router.Match("/:controller(/:action", new DummyHandlerMediator());
		}
		
		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_4()
		{
			var router = new Router();
			router.Match("/:controller(/:action)/)", new DummyHandlerMediator());
		}

		[TestMethod, ExpectedException(typeof(RouteParsingException))]
		public void DefiningRoute_InvalidPath_5()
		{
			var router = new Router();
			router.Match("/:controller((/:action)", new DummyHandlerMediator());
		}

		[TestMethod]
		public void Foo()
		{
			Bar();
		}

		public void Bar([Optional] int opt)
		{
			
		}
	}
}
