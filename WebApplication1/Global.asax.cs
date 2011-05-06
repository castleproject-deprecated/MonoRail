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
			Router.Instance.Match("/:controller(/:action(/:id))", "default", 
                c => c.Defaults(d => d.Controller("home").Action("index")));
		}
	}
}
