namespace TestWebApp
{
	using System;
	using System.Web.Routing;
	using Castle.MonoRail3.Hosting.Mvc;

	public class Global : System.Web.HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
			RouteTable.Routes.Add(
				new Route("{controller}/{action}",
				          new RouteValueDictionary(new {controller = "home", action = "index"}),
				          new MvcRouteHandler()));
		}
	}
}
