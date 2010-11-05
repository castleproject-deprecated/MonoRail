namespace Castle.MonoRail3.Hosting.Mvc
{
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;

	[Export]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class RequestParser
	{
		public virtual RouteData ParseDescriminators(HttpRequestBase request)
		{
			return request.RequestContext.RouteData;
		}
	}
}