namespace Castle.MonoRail3.Primitives
{
	using System.Web;
	using System.Web.Routing;
	using Mvc;

	public abstract class ControllerExecutorProvider
	{
		public abstract ControllerExecutor CreateExecutor(ControllerMeta meta, RouteData data, HttpContextBase context);
	}
}
