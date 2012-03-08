namespace WebSiteForIntegration
{
	using Castle.MonoRail;
	using Castle.MonoRail.Routing;

	public class Global : MrBasedHttpApplication
	{
		public override void ConfigureRoutes(Router router)
		{
			router.Match("(/:controller(/:action(/:id)))", "default",
								  c => c.Defaults(d => d.Controller("root").Action("index")))
								  // .SetFilter<BeforeActionFilter>()
								  // .SetFilter<AfterActionFilter>()
								  ;

			router.Match("/viewcomponents/:controller(/:action(/:id))",
								  c =>
								  c.Match("(/:area/:controller(/:action(/:id)))", "viewcomponents",
										  ic => ic.Defaults(d => d.Action("index"))));
		}
	}
}
