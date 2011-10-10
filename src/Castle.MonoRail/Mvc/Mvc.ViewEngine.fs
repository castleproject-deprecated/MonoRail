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

namespace Castle.MonoRail.ViewEngines

    open System
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.IO
    open System.Web

    // optional extension point to allow for custom layouts in projects (is it worthwhile?)
    [<Interface>]
    type IViewFolderLayout = 
        abstract member ProcessLocations : req:ViewRequest * http:HttpContextBase -> unit
        abstract member ProcessPartialLocations : req:ViewRequest * http:HttpContextBase -> unit


    [<System.ComponentModel.Composition.Export(typeof<IViewFolderLayout>)>]
    type DefaultViewFolderLayout 
        [<ImportingConstructor>] ([<Import("AppPath")>] appPath:string) = 
        
        let compute_view_locations areaname (viewname:string) (controller:string) = 
            let hasSlash = viewname.IndexOf '/' <> -1
            let spec_view = 
                if areaname != null then 
                    [
                        Helpers.path_combine appPath (areaname + "/Views/" + (if hasSlash then viewname else controller + "/" + viewname))
                        Helpers.path_combine appPath ("/Views/" + areaname + "/" + (if hasSlash then viewname else controller + "/" + viewname))
                    ]
                else 
                    [
                        Helpers.path_combine appPath ("/Views/" + (if hasSlash then viewname else controller + "/" + viewname))
                    ]
            let shared_view = 
                if areaname != null then 
                    [
                        Helpers.path_combine appPath "/" + areaname + "/Views/Shared/" + viewname
                        Helpers.path_combine appPath "/Views/" + areaname + "/Shared/" + viewname
                    ]
                else 
                    [
                        Helpers.path_combine appPath "/Views/Shared/" + viewname
                    ]
            spec_view @ shared_view
        
        let compute_layout_locations areaname (layout:string) (controller:string) = 
            let hasSlash = layout.IndexOf '/' <> -1
            let lpath = 
                if areaname != null then 
                    [
                        Helpers.path_combine appPath "/" + areaname + "/Views/" + (if hasSlash then layout else controller + "/" + layout)
                        Helpers.path_combine appPath "/Views/" + areaname + "/" + (if hasSlash then layout else controller + "/" + layout)
                    ]
                else 
                    [
                        Helpers.path_combine appPath "/Views/" + (if hasSlash then layout else controller + "/" + layout)
                    ]
            let lshared = 
                if areaname != null then 
                    [
                        Helpers.path_combine appPath "/" + areaname + "/Views/Shared/" + layout
                        Helpers.path_combine appPath "/Views/" + areaname + "/Shared/" + layout
                    ]
                else 
                    [
                        "/Views/Shared/" + layout
                    ]
            lpath @ lshared

        interface IViewFolderLayout with
            member x.ProcessLocations (req:ViewRequest, http:System.Web.HttpContextBase) = 
                if not req.WasProcessed then
                    if req.ViewName == null then
                        req.ViewName <- req.DefaultName
                    req.ViewLocations <- compute_view_locations req.GroupFolder req.ViewName req.ViewFolder
                    let layout = req.OuterViewName
                    if (layout != null) then 
                        req.LayoutLocations <- compute_layout_locations req.GroupFolder layout req.ViewFolder
                    req.SetProcessed()

            member x.ProcessPartialLocations (req:ViewRequest, http:System.Web.HttpContextBase) = 
                if not req.WasProcessed then
                    if req.ViewName == null then
                        req.ViewName <- req.DefaultName
                    req.ViewLocations <- compute_view_locations req.GroupFolder req.ViewName req.ViewFolder
                    req.SetProcessed()



    [<Export>]
    type ViewRendererService () =
        // [<ImportingConstructor>] ([<Import("AppPath")>] appPath:string) =
        let mutable _viewEngines = System.Linq.Enumerable.Empty<IViewEngine>()
        let mutable _viewFolderLayout = Unchecked.defaultof<IViewFolderLayout>

        let rec find_ve_r viewLocations layoutLocations (enumerator:IEnumerator<IViewEngine>) (reslist:List<ViewEngineResult>) : List<ViewEngineResult> =
            if enumerator.MoveNext() then
                let item = enumerator.Current
                let res = item.ResolveView (viewLocations, layoutLocations)
                if (res.IsSuccessful) then
                    reslist.Clear()
                    reslist.Add res
                    reslist
                else
                    reslist.Add res
                    find_ve_r viewLocations layoutLocations enumerator reslist
            else 
                reslist

        and find_ve viewLocations layoutLocations (viewengines:IViewEngine seq) : ViewEngineResult = 
            use enumerator = viewengines.GetEnumerator()
            let searched = List<ViewEngineResult>()
            let results = find_ve_r viewLocations layoutLocations enumerator searched

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

        member x.HasView (viewreq:ViewRequest, context:HttpContextBase) = 
            _viewFolderLayout.ProcessLocations (viewreq, context)
            x.InternalHasView(viewreq)

        member x.HasPartialView (viewreq:ViewRequest, context:HttpContextBase) = 
            _viewFolderLayout.ProcessPartialLocations (viewreq, context)
            x.InternalHasView(viewreq)

        member internal x.InternalHasView(viewreq:ViewRequest) = 
            let result = _viewEngines |> Seq.tryFindIndex (fun ve -> ve.HasView(viewreq.ViewLocations) )
            result.IsSome

        member x.RenderPartial (viewreq:ViewRequest, context:HttpContextBase, propbag:IDictionary<string,obj>, model, output:TextWriter) =
            _viewFolderLayout.ProcessPartialLocations (viewreq, context)

            let res = find_ve (viewreq.ViewLocations) null _viewEngines
            let view = res.View
            let viewCtx = ViewContext(context, propbag, model, viewreq)
            view.Process (output, viewCtx)

        member x.Render (viewreq:ViewRequest, context:HttpContextBase, propbag:IDictionary<string,obj>, model, output:TextWriter) = 
            _viewFolderLayout.ProcessLocations (viewreq, context)
        
            let res = find_ve (viewreq.ViewLocations) (viewreq.LayoutLocations) _viewEngines
            let view = res.View
            let viewCtx = ViewContext(context, propbag, model, viewreq)
            view.Process (output, viewCtx)

        member x.Render (viewreq:ViewRequest, context:HttpContextBase, propbag:IDictionary<string,obj>, model) = 
            x.Render (viewreq, context, propbag, model, (context.Response.Output))


