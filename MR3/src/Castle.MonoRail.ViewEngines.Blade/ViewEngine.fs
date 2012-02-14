//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.ViewEngines.Blade

    open System
    open System.Collections.Generic
    open System.Linq
    open System.ComponentModel.Composition
    open System.Web.Compilation
    open Castle.Blade.Web
    open Castle.MonoRail
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Helpers
    open Castle.MonoRail.Resource
    open Castle.MonoRail.ViewEngines
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Castle.MonoRail.Blade

    [<Export(typeof<IViewEngine>)>]
    [<ExportMetadata("Order", 10000)>]
    type BladeViewEngine [<ImportingConstructor>] ([<Import("AppPath")>] appPath:string) = 
        inherit BaseViewEngine()

        let mutable _hosting = Unchecked.defaultof<IAspNetHostingBridge>
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()
        let mutable _registry : IServiceRegistry = Unchecked.defaultof<_>
        let _deploymentInfo : Ref<IDeploymentInfo> = ref null

        static member Initialize() = 
            BuildProvider.RegisterBuildProvider(".cshtml", typeof<Castle.Blade.Web.BladeBuildProvider>);

        [<Import(AllowDefault=true)>]
        member x.DeploymentInfo with get() = !_deploymentInfo and set(v) = _deploymentInfo := v

        [<Import>]
        member x.HostingBridge
            with get() = _hosting and set v = _hosting <- v

        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders
            with get() = _resProviders and set v = _resProviders <- v

        [<Import>]
        member x.ServiceRegistry
            with get() = _registry and set v = _registry <- v

        override this.HasView (viewLocations) = 
            Assertions.ArgNotNull viewLocations "viewLocations"
            let views = seq {
                                for l in viewLocations do
                                    yield l + ".cshtml"
                            }
            let existing_views, _ = base.FindProvider views
            (existing_views <> null)

        override this.ResolveView (viewLocations, layoutLocations) = 
            Assertions.ArgNotNull viewLocations "viewLocations"

            let views = seq {
                                for l in viewLocations do
                                    yield l + ".cshtml"
                            }

            let layouts = seq {
                                if layoutLocations != null then 
                                    for l in layoutLocations do
                                        yield l + ".cshtml"
                              } 

            let existing_views, provider1 = this.FindProvider views
            let layout, provider2 = this.FindProvider layouts

            if (existing_views != null) then
                let view = Seq.head existing_views
                let layout = (if layout != null then Seq.head layout else null)
                let contextualRoot =  
                    if !_deploymentInfo <> null then (!_deploymentInfo).VirtualPath else appPath 
                let razorview = BladeView(view, layout, _hosting, _registry, appPath, contextualRoot)
                ViewEngineResult(razorview, this)
            else
                ViewEngineResult()


    and
        BladeView(viewPath, layoutPath, hosting, registry, appRoot, contextualAppRoot) = 
            let _viewInstance = 
                lazy (
                        let compiled = hosting.GetCompiledType(viewPath)
                        System.Activator.CreateInstance(compiled) 
                     )
            let _layoutPath = layoutPath
            let _viewPath = viewPath

            interface IView with
                member x.Process (writer, viewctx) = 
                    let instance = _viewInstance.Force()
                    let pageBase = instance :?> WebBladePage

                    match instance with 
                    | :? IViewPage as vp -> 
                        
                        if (_layoutPath != null) then 
                            vp.Layout <- "~" + _layoutPath
                        
                        vp.ViewContext <- viewctx
                        vp.RawModel <- viewctx.Model
                        vp.Bag <- viewctx.Bag
                        vp.HelperContext <- HelperContext(viewctx, registry)
                        vp.AppRoot <- appRoot
                        vp.ContextualAppRoot <- contextualAppRoot

                    | _ -> 
                        failwith "Generated page does not implement IViewPage"
                        
                    let pageCtx = PageContext(viewctx.HttpContext, "~" + _viewPath)
                    pageBase.RenderPage(pageCtx, writer)

