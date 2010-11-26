//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
namespace TestWebApp
{
	using System;
	using System.Web;
	using System.Web.Routing;
	using Castle.MicroKernel.Registration;
	using Castle.MonoRail.Mvc;
	using Castle.Windsor;
	using Controller;

	public class Global : HttpApplication, IContainerAccessor
	{
		private static WindsorContainer _container;

		void Application_Start(object sender, EventArgs e)
		{
			_container = new WindsorContainer();
			_container.Register(AllTypes.
				FromAssembly(typeof(Global).Assembly).
				Where(t => t.Name.EndsWith("Controller")).
				Configure(t => t.Named(t.Implementation.Name.Substring(0, t.Implementation.Name.Length - "Controller".Length).ToLowerInvariant()).
					LifeStyle.Transient));

			RouteTable.Routes.Add(
				new Route("{controller}/{action}/{id}",
						  new RouteValueDictionary(new {controller = "home", action = "index", id = ""}),
						  new MvcRouteHandler()));
		}

		public IWindsorContainer Container
		{
			get { return _container; }
		}
	}
}
