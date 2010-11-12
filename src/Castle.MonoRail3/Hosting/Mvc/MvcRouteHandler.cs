namespace Castle.MonoRail3.Hosting.Mvc
{
	using System.Web;
	using System.Web.Routing;

	public class MvcRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new ComposableMvcHandler();
		}
	}
}
