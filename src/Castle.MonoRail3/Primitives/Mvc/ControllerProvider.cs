namespace Castle.MonoRail3.Primitives.Mvc
{
    using System.Web;
    using System.Web.Routing;

    public abstract class ControllerProvider
    {
        public abstract ControllerMeta Create(RouteData data, HttpContextBase context);
    }
}
