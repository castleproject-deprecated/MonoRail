namespace Castle.MonoRail.Hosting.Mvc
{
	using System.IO;
	using System.Web;

	public class ViewContext
	{
		public ViewContext(HttpContextBase httpContext, TextWriter writer)
		{
			HttpContext = httpContext;
			Writer = writer;
		}

		public TextWriter Writer { get; private set; }
		public HttpContextBase HttpContext { get; private set; }
	}
}
