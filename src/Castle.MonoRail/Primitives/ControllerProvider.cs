namespace Castle.MonoRail.Primitives
{
	using System.Web;
	using System.Web.Routing;
	using Mvc;

	public abstract class ControllerProvider
    {
        public abstract ControllerMeta Create(RouteData data);
    }
}
