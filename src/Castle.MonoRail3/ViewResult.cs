namespace Castle.MonoRail3
{
	using System;
	using System.Web;
	using Hosting.Mvc;
	using Primitives.Mvc;

	public class ViewResult : ActionResult
	{
		private readonly string viewName;

		public ViewResult(string viewName)
		{
			this.viewName = viewName;
		}

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			var viewEngines = services.ViewEngines;

			var result = viewEngines.ResolveView(viewName, null, new ViewResolutionContext(context));

			if (result.Successful)
			{
				try
				{
					result.View.Process(
						new ViewContext(
							new HttpContextWrapper(HttpContext.Current), HttpContext.Current.Response.Output),
						HttpContext.Current.Response.Output);
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
