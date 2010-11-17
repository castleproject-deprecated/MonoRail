namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Web;
	using System.Web.WebPages;
	using Hosting.Internal;
	using Hosting.Mvc;
	using Primitives.Mvc;

	public class RazorView : IView
	{
		private readonly IHostingBridge hostingBridge;
		
		public RazorView(IHostingBridge hostingBridge, string view, string layout)
		{
			ViewPath = view;
			LayoutPath = layout;

			this.hostingBridge = hostingBridge;
		}

		private string LayoutPath { get; set; }

		private string ViewPath { get; set; }

		protected internal virtual object CreateViewInstance()
		{
			Type compiledType = hostingBridge.GetCompiledType(ViewPath);

			return Activator.CreateInstance(compiledType);
		}

		public void Process(ViewContext viewContext, TextWriter writer)
		{
			object view = CreateViewInstance();
			if (view == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"View could not be created : {0}", ViewPath));
			}

			WebViewPage initPage = view as WebViewPage;
			if (initPage == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"wrong base type for view: {0}", ViewPath));
			}

			//initPage.OverridenLayoutPath = this.LayoutPath;
			initPage.VirtualPath = this.ViewPath;
			initPage.Context = viewContext.HttpContext;
			initPage.Data = viewContext.Data;
			//initPage.InitHelpers();

			initPage.ExecutePageHierarchy(new WebPageContext(viewContext.HttpContext, initPage, null), writer, initPage);
		}
	}
}