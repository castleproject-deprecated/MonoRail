using System.Web;

namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System.ComponentModel.Composition;
	using System.Web.Compilation;
	using System.Web.WebPages.Razor;
	using Hosting.Internal;
	using Hosting.Mvc;
	using MonoRail.Hosting.Mvc;

	[Export(typeof(IViewEngine))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class RazorViewEngine : VirtualPathProviderViewEngine
	{
		[Import]
		public IHostingBridge HostingBridge { get; set; }

		public RazorViewEngine()
		{
			AreaViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
			AreaLayoutLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
			AreaPartialViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
			ViewLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml"
            };
			LayoutLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml"
            };
			PartialViewLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml"
            };
		}

		protected override IView CreateView(string viewPath, string layoutPath)
		{
			return new RazorView(HostingBridge, viewPath, layoutPath);
		}

		protected override bool FileExists(string path)
		{
			return HostingBridge.FileExists(path);
		}

		public static void Initialize()
		{
			BuildProvider.RegisterBuildProvider(".cshtml", typeof(RazorBuildProvider));
		}
	}
}