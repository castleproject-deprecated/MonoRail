
namespace Castle.MonoRail.Mvc.ViewEngines.Razor

    open System.Web.WebPages.Razor

    type MonoRailRazorHostFactory() = 
        inherit WebRazorHostFactory() 

        override x.CreateHost (vpath, ppath) = 
            let host = base.CreateHost(vpath, ppath)

            if not host.IsSpecialPage then
                upcast MonoRailRazorHost(vpath, ppath)
            else
                host

    and MonoRailRazorHost (vpath, ppath) as self =
        inherit WebPageRazorHost(vpath, ppath)

        let remove_ns names =
            for n in names do
                self.NamespaceImports.Remove n |> ignore

        do
            base.DefaultPageBaseClass <- typeof<Castle.MonoRail.Razor.WebViewPage>.FullName;
            // RemoveNamespace "WebMatrix.Data", "System.Web.WebPages.Html", "WebMatrix.WebData" 



