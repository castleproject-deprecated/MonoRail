namespace Castle.MonoRail.Hosting.Mvc
{
    using System.ComponentModel.Composition;
    using Internal;

    [Export(typeof(IViewEngine))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WebFormsViewEngine : VirtualPathProviderViewEngine
    {
        [Import]
        public IHostingBridge HostingBridge { get; set; }

        [Import]
        public IWebFormFactory WebFormFactory { get; set; }

        public WebFormsViewEngine()
        {
            LayoutLocationFormats = new[] {
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master"
            };

            AreaLayoutLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.master",
                "~/Areas/{2}/Views/Shared/{0}.master",
            };

            ViewLocationFormats = new[] {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            AreaViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            PartialViewLocationFormats = ViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;
        }

        protected override IView CreateView(string viewPath, string layoutPath)
        {
            return new WebFormView(this.WebFormFactory, viewPath, layoutPath);
        }

        protected override bool FileExists(string path)
        {
            return HostingBridge.FileExists(path);
        }
    }

}
