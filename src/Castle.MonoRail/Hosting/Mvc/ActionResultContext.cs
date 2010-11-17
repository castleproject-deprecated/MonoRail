namespace Castle.MonoRail.Primitives.Mvc
{
	using System.Web;

	public class ActionResultContext : BaseMvcContext
    {
		public HttpContextBase HttpContext { get; set; }

		public ActionResultContext(string areaName, string controllerName, string actionName, HttpContextBase httpContext) : 
            base(areaName, controllerName, actionName)
        {
        	HttpContext = httpContext;
        }
    }
}