//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Mvc.ViewEngines.Razor

    open System
    open System.Collections.Generic
    open System.Linq
    open System.ComponentModel.Composition
    open System.Web.Compilation
    open System.Web.WebPages
    open Castle.MonoRail.Razor
    open Castle.MonoRail.Resource
    open Castle.MonoRail.Mvc.ViewEngines
    open Castle.MonoRail.Hosting.Mvc.Typed

    [<Export(typeof<IViewEngine>)>]
    [<ExportMetadata("Order", 100000)>]
    type RazorViewEngine() = 
        inherit BaseViewEngine()

        let mutable _hosting = Unchecked.defaultof<IAspNetHostingBridge>
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()

        static member Initialize() = 
            BuildProvider.RegisterBuildProvider(".cshtml", typeof<System.Web.WebPages.Razor.RazorBuildProvider>);
            BuildProvider.RegisterBuildProvider(".vbhtml", typeof<System.Web.WebPages.Razor.RazorBuildProvider>);

        [<Import>]
        member x.HostingBridge 
            with get() = _hosting and set v = _hosting <- v

        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders 
            with get() = _resProviders and set v = _resProviders <- v

        override this.ResolveView (viewLocations, layoutLocations) = 
            let views = seq {
                                for l in viewLocations do
                                    yield l + ".cshtml"
                                    yield l + ".vbhtml"
                            }

            let layouts = seq {
                                if layoutLocations != null then 
                                    for l in layoutLocations do
                                        yield l + ".cshtml"
                                        yield l + ".vbhtml"
                              } 

            let existing_views, provider1 = this.FindProvider views
            let layout, provider2 = this.FindProvider layouts

            if (existing_views != null) then
                let razorview = RazorView(existing_views, (if layout != null then Seq.head layout else null), _hosting)
                ViewEngineResult(razorview, this)
            else
                ViewEngineResult()


    and
        RazorView(viewPath, layoutPath, hosting) = 
            let _viewInstance = lazy (
                    let compiled = hosting.GetCompiledType(Seq.head viewPath)
                    System.Activator.CreateInstance(compiled) 
                )
            let _layoutPath = layoutPath
            let _viewPath = viewPath

            interface IView with
                member x.Process (writer, viewctx) = 
                    let instance = _viewInstance.Force()

                    match instance with 
                    | :? IViewPage as vp -> 
                        
                        if (_layoutPath != null) then 
                            vp.Layout <- "~" + _layoutPath
                        
                        vp.ViewContext <- viewctx
                        vp.RawModel <- viewctx.Model
                        vp.Bag <- viewctx.Bag

                    | _ -> 
                        failwith "Wrong base type... "
                        
                    let pageBase = instance :?> WebPageBase
                    pageBase.VirtualPath <- "~" + Seq.head _viewPath
                    pageBase.Context <- viewctx.HttpContext

                    (*
			        initPage.ViewComponentRenderer = viewComponentRenderer;
                    *)

                    let pageCtx = WebPageContext(viewctx.HttpContext, pageBase, viewctx.Model)
                    pageBase.ExecutePageHierarchy(pageCtx, writer, pageBase)

                



