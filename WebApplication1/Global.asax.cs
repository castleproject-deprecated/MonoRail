using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApplication1
{
	using Castle.MonoRail.Routing;
	using WebApplication1.Controllers;

	public class Global : System.Web.HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
			// Router.Instance.Match("(/:controller(/:action(/:id)))");
			Router.Instance.Match("/:controller(/:action(/:id))", 
                c => c.Defaults(d => d.Controller("home").Action("index")));
				// , 
				// cfg => cfg.Defaults( d => d.Controller<HomeController> ));
		}
	}

	internal class Mediator : IRouteHttpHandlerMediator, IHttpHandler
	{
		private RouteMatch _data;

		public IHttpHandler GetHandler(HttpRequest obj0, RouteMatch data)
		{
			_data = data;
			return this;
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.Write("Hello " + _data.RouteParams.Count);
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
