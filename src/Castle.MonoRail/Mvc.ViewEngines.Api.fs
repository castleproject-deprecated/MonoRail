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
    open System.Web
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

        member this.ViewName
            with get() = _viewName and set v = _viewName <- v
        member this.LayoutName
            with get() = _layout and set v = _layout <- v
        member this.AreaName 
            with get() = _area and set v = _area <- v
        member this.ControllerName 
            with get() = _controller and set v = _controller <- v
        member this.ActionName
            with get() = _action and set v = _action <- v
        member this.ViewLocations
            with get() = _viewLocations and set v = _viewLocations <- v
        member this.LayoutLocations
            with get() = _layoutLocations and set v = _layoutLocations <- v


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
            abstract member ResolveView : viewLocations:string seq -> ViewEngineResult


    and [<Interface>] 
        public IView =
            abstract member Process : writer:TextWriter * httpctx:HttpContextBase -> unit

    // optional extension point to allow for custom layouts in projects (is it worthwhile?)
    [<Interface>]
    type IViewFolderLayout = 
        abstract member ProcessLocations : req:ViewRequest * http:HttpContextBase -> unit

    [<System.ComponentModel.Composition.Export(typeof<IViewFolderLayout>)>]
    type DefaultViewFolderLayout() = 
        
        interface IViewFolderLayout with
            member x.ProcessLocations (req:ViewRequest, http:System.Web.HttpContextBase) = 
                if req.ViewName == null then
                    req.ViewName <- req.ActionName 

                let view = req.ViewName
                let hasSlash = view.IndexOf '/' <> -1

                let path = 
                    if req.AreaName != null then 
                        req.AreaName + "/Views/" + (if hasSlash then view else req.ControllerName + "/" + view) 
                    else 
                        "/Views/" + (if hasSlash then view else req.ControllerName + "/" + view)

                let shared = 
                    if req.AreaName != null then 
                        req.AreaName + "/Views/Shared/" + view 
                    else 
                        "/Views/Shared/" + view

                req.ViewLocations <- [path;shared] 

                let layout = req.LayoutName
                
                if (layout != null) then 
                    let layouthasSlash = layout.IndexOf '/' <> -1
                    let lpath = 
                        if req.AreaName != null then 
                            req.AreaName + "/Views/" + (if hasSlash then view else req.ControllerName + "/" + layout) 
                        else 
                            "/Views/" + (if hasSlash then view else req.ControllerName + "/" + layout)

                    let lshared = 
                        if req.AreaName != null then 
                            req.AreaName + "/Views/Shared/" + layout
                        else 
                            "/Views/Shared/" + layout

                    req.LayoutLocations <- [lpath;lshared]


