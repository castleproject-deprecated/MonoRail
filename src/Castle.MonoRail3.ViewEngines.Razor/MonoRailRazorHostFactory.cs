namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System.Web.WebPages.Razor;

	public class MonoRailRazorHostFactory : WebRazorHostFactory
	{
		public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath = null)
		{
			WebPageRazorHost host = base.CreateHost(virtualPath, physicalPath);

			if (!host.IsSpecialPage)
			{
				return new MonoRailRazorHost(virtualPath, physicalPath);
			}

			return host;
		}
	}
}
