using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApplication1
{
	using Castle.MonoRail.Routing;

	public class Global : System.Web.HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
			Router.Instance.Match("(/:controller(/:action(/:id)))");
		}

		void Application_End(object sender, EventArgs e)
		{
			//  Code that runs on application shutdown

		}

		void Application_Error(object sender, EventArgs e)
		{
			// Code that runs when an unhandled error occurs

		}

		void Session_Start(object sender, EventArgs e)
		{
			// Code that runs when a new session is started

		}

		void Session_End(object sender, EventArgs e)
		{
			// Code that runs when a session ends. 
			// Note: The Session_End event is raised only when the sessionstate mode
			// is set to InProc in the Web.config file. If session mode is set to StateServer 
			// or SQLServer, the event is not raised.

		}

	}

	internal class Mediator : IRouteHttpHandlerMediator, IHttpHandler
	{
		private RouteData _data;

		public IHttpHandler GetHandler(HttpRequest obj0, RouteData data)
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
