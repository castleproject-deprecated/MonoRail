namespace WebApplication1
{
    using System;
    using Castle.MonoRail;
    using Castle.MonoRail.Routing;
    using Filters;

	public class Global : System.Web.HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
            Router.Instance.Match("(/:controller(/:action(/:id)))", "default", 
								  c => c.Defaults(d => d.Controller("todo").Action("index")))
								  .SetFilter<BeforeActionFilter>(ExecuteWhen.Before)
								  .SetFilter<AfterActionFilter>(ExecuteWhen.After);
		}
	}
}
