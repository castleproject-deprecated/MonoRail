namespace Castle.MonoRail3.Primitives
{
	using System.Web;
	using System.Web.Routing;

	public sealed class ControllerExecutionContext
	{
		internal ControllerExecutionContext(HttpContextBase httpContext, 
			object controller, RouteData data, 
			ControllerDescriptor controllerDescriptor)
		{
			HttpContext = httpContext;
			Controller = controller;
			RouteData = data;
			ControllerDescriptor = controllerDescriptor;
		}

		// readonly
		public object Controller { get; private set; }
		public HttpContextBase HttpContext { get; private set; }
		public RouteData RouteData { get; private set; }
		public ControllerDescriptor ControllerDescriptor { get; private set; }

		// writable
		public ActionDescriptor SelectedAction { get; set; }
		public object InvocationResult { get; set; }
	}
}