namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System.Web.WebPages.Razor;

	public class MonoRailRazorHost : WebPageRazorHost
	{
		public MonoRailRazorHost(string virtualPath, string physicalPath)
			: base(virtualPath, physicalPath)
		{

			//RegisterSpecialFile(RazorViewEngine.ViewStartFileName, typeof(ViewStartPage));

			DefaultPageBaseClass = typeof(WebViewPage).FullName;

			// REVIEW get rid of the namespace import to not force additional references in default MVC projects
			if (NamespaceImports.Contains("System.Web.WebPages.Html"))
			{
				NamespaceImports.Remove("System.Web.WebPages.Html");
			}

			if (NamespaceImports.Contains("WebMatrix.Data"))
			{
				NamespaceImports.Remove("WebMatrix.Data");
			}
		}
	}
}