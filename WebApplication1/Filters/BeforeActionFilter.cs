namespace WebApplication1.Filters
{
	using System.Web;
	using Castle.MonoRail.Filter;

	public class BeforeActionFilter : IFilter
	{
		public bool Execute(object controller, HttpContextBase context)
		{
			context.Response.Write("<!-- Before action filter -->");

			return true;
		}
	}

	public class AfterActionFilter : IFilter
	{
		public bool Execute(object controller, HttpContextBase context)
		{
			context.Response.Write("<!-- After action filter -->");

			return true;
		}
	}
}