namespace Castle.MonoRail3.Primitives.Mvc
{
	using System.Web;
	using System.Web.Routing;

	public abstract class ControllerExecutorProvider
	{
		public abstract ControllerExecutor CreateExecutor(ControllerMeta meta, RouteData data, HttpContextBase context);
	}
}
