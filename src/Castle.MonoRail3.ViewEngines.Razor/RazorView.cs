namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Web.WebPages;
	using Hosting.Internal;
	using Hosting.Mvc;
	using MonoRail.Hosting.Mvc;

	public class RazorView : IView
	{
		private readonly IHostingBridge _hostingBridge;
		private string LayoutPath { get; set; }
		private string ViewPath { get; set; }

		public RazorView(IHostingBridge hostingBridge, string view, string layout)
		{
			this.ViewPath = view;
			this.LayoutPath = layout;
			this._hostingBridge = hostingBridge;
		}

		protected internal virtual object CreateViewInstance()
		{
			Type compiledType = this._hostingBridge.GetCompiledType(this.ViewPath);
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
			//            initPage.ViewContext = viewContext;
			//            initPage.ViewData = viewContext.ViewData;
			initPage.InitHelpers();
			initPage.ExecutePageHierarchy(new WebPageContext(), writer, initPage);
		}
	}
}