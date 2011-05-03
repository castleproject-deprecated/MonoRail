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

namespace Castle.MonoRail.Mvc.ViewEngines

    open System
    open System.IO
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.Linq
    open System.Web
    open Castle.MonoRail.Resource
    open Helpers

    // we need to make sure this interface allows for recursive view engines
    // ie. view engines that would allow for application of layouts recursively with no change needed in its api

    type ViewRequest() = 
        let mutable _area : string = null
        let mutable _controller : string = null
        let mutable _action : string = null
        let mutable _viewName : string = null
        let mutable _layout : string = null
        let mutable _viewLocations : string seq = null
        let mutable _layoutLocations : string seq = null

        member this.ViewName with get() = _viewName and set v = _viewName <- v
        member this.LayoutName with get() = _layout and set v = _layout <- v
        member this.AreaName with get() = _area and set v = _area <- v
        member this.ControllerName with get() = _controller and set v = _controller <- v
        member this.ActionName with get() = _action and set v = _action <- v
        member this.ViewLocations with get() = _viewLocations and set v = _viewLocations <- v
        member this.LayoutLocations with get() = _layoutLocations and set v = _layoutLocations <- v


    type ViewEngineResult(view:IView, engine:IViewEngine) = 
        let _view = view
        let _engine = engine

        new () = 
            ViewEngineResult(Unchecked.defaultof<_>, Unchecked.defaultof<_>)

        member x.View = _view
        member x.Engine = _engine
        member x.IsSuccessful = _view != null
    

    and [<Interface>] 
        public IViewEngine =
            abstract member ResolveView : viewLocations:string seq * layoutLocations:string seq -> ViewEngineResult


    and [<Interface>] 
        public IView =
            abstract member Process : writer:TextWriter * ctx:ViewContext -> unit

    and 
        public ViewContext(httpctx:HttpContextBase, model) = 
            let _httpctx = httpctx
            let _model = model
            let mutable _writer : TextWriter = _httpctx.Response.Output

            member x.HttpContext = _httpctx
            member x.Model = _model
            member x.Writer  with get() = _writer and set v = _writer <- v


    [<AbstractClass>]
    type BaseViewEngine() = 
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()

        let rec provider_sel (enumerator:IEnumerator<ResourceProvider>) paths : string * ResourceProvider = 
            if (enumerator.MoveNext()) then
                let provider = enumerator.Current
                let existing_view = 
                    paths
                    |> Seq.find (fun (v) -> provider.Exists(v)) 
                if existing_view = null then 
                    provider_sel enumerator paths
                else 
                    existing_view, provider
            else
                null, Unchecked.defaultof<_>

        and find_provider paths = 
            use enumerator = _resProviders.GetEnumerator()
            provider_sel enumerator paths 
            
        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders 
            with get() = _resProviders and set v = _resProviders <- v

        member x.FindProvider(paths) = 
            find_provider paths

        abstract member ResolveView : viewLocations:string seq * layoutLocations:string seq -> ViewEngineResult

        interface IViewEngine with 
            member this.ResolveView (viewLocations, layoutLocations) = 
                this.ResolveView(viewLocations, layoutLocations)


    // optional extension point to allow for custom layouts in projects (is it worthwhile?)
    [<Interface>]
    type IViewFolderLayout = 
        abstract member ProcessLocations : req:ViewRequest * http:HttpContextBase -> unit


    [<System.ComponentModel.Composition.Export(typeof<IViewFolderLayout>)>]
    type DefaultViewFolderLayout() = 
        
        let compute_view_locations areaname (viewname:string) (controller:string) = 
            let hasSlash = viewname.IndexOf '/' <> -1
            let spec_view = 
                if areaname != null then 
                    areaname + "/Views/" + (if hasSlash then viewname else controller + "/" + viewname) 
                else 
                    "/Views/" + (if hasSlash then viewname else controller + "/" + viewname)
            let shared_view = 
                if areaname != null then 
                    areaname + "/Views/Shared/" + viewname 
                else 
                    "/Views/Shared/" + viewname
            [spec_view;shared_view]
        
        let compute_layout_locations areaname (layout:string) (controller:string) = 
            let hasSlash = layout.IndexOf '/' <> -1
            let lpath = 
                if areaname != null then 
                    areaname + "/Views/" + (if hasSlash then layout else controller + "/" + layout) 
                else 
                    "/Views/" + (if hasSlash then layout else controller + "/" + layout)
            let lshared = 
                if areaname != null then 
                    areaname + "/Views/Shared/" + layout
                else 
                    "/Views/Shared/" + layout
            [lpath;lshared]

        interface IViewFolderLayout with
            member x.ProcessLocations (req:ViewRequest, http:System.Web.HttpContextBase) = 
                if req.ViewName == null then
                    req.ViewName <- req.ActionName 
                req.ViewLocations <- compute_view_locations req.AreaName req.ViewName req.ControllerName
                let layout = req.LayoutName
                if (layout != null) then 
                    req.LayoutLocations <- compute_layout_locations req.AreaName layout req.ControllerName


    [<Export>]
    type ViewRendererService() = 
        let mutable _viewEngines = System.Linq.Enumerable.Empty<IViewEngine>()
        let mutable _viewFolderLayout = Unchecked.defaultof<IViewFolderLayout>

        let rec find_ve_r (viewLocations, layoutLocations, enumerator:IEnumerator<IViewEngine>, reslist:List<ViewEngineResult>) : List<ViewEngineResult> =
            if enumerator.MoveNext() then
                let item = enumerator.Current
                let res = item.ResolveView (viewLocations, layoutLocations)
                if (res.IsSuccessful) then
                    reslist.Clear()
                    reslist.Add res
                    reslist
                else
                    reslist.Add res
                    find_ve_r (viewLocations, layoutLocations, enumerator, reslist)
            else 
                reslist

        and find_ve viewLocations layoutLocations (viewengines:IViewEngine seq) : ViewEngineResult = 
            use enumerator = viewengines.GetEnumerator()
            let results = find_ve_r(viewLocations, layoutLocations, enumerator, (List<ViewEngineResult>()))

            if Seq.isEmpty(results) then
                failwith "no view engines? todo: decent error msg"
            else 
                let h = Seq.head(results)
                if (h.IsSuccessful) then
                    h
                else 
                    failwith "todo: decent error msg"

        [<ImportMany(AllowRecomposition=true)>]
        member x.ViewEngines  with set v = _viewEngines <- v

        [<Import>]
        member x.ViewFolderLayout  with set v = _viewFolderLayout <- v

        member x.Render(viewreq:ViewRequest, context:HttpContextBase, model) = 
            _viewFolderLayout.ProcessLocations (viewreq, context)
        
            let res = find_ve (viewreq.ViewLocations) (viewreq.LayoutLocations) _viewEngines
        
            let view = res.View
        
            let viewCtx = ViewContext(context, model)

            view.Process (context.Response.Output, viewCtx)

