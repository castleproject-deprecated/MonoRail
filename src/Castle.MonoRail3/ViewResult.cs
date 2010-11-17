namespace Castle.MonoRail3
{
	using System;
	using System.Web;
	using Hosting.Mvc;
	using Primitives.Mvc;

	public class ViewResult : ActionResult
	{
		private readonly string viewName;

		public ViewResult(string viewName, dynamic data = null)
		{
			Data = data;
			this.viewName = viewName;
		}

		public dynamic Data { get; set; }

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			var viewEngines = services.ViewEngines;

			var result = viewEngines.ResolveView(viewName, null, new ViewResolutionContext(context));

			if (result.Successful)
			{
				try
				{
					//TODO: needs a better way to resolve the HttpContext
					var httpContext = new HttpContextWrapper(HttpContext.Current);
					var viewContext = new ViewContext(httpContext, httpContext.Response.Output) {Data = Data};

					result.View.Process(viewContext, httpContext.Response.Output);
				}
				finally
				{
					result.ViewEngine.Release(result.View);
				}
			}
			else
			{
				throw new Exception("Could not find view " + viewName +
					". Searched at " + string.Join(", ", result.SearchedLocations));
			}
		}
	}
}
