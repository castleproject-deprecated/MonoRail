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

			RemoveNamespace("WebMatrix.Data", "System.Web.WebPages.Html", "WebMatrix.WebData");
		}

		private void RemoveNamespace(params string[] namespaces)
		{
			foreach (var ns in namespaces)
			{
				if (NamespaceImports.Contains(ns))
				{
					NamespaceImports.Remove(ns);
				}
			}
		}
	}
}