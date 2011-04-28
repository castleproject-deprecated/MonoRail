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

namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Web
    open Castle.MonoRail.Mvc.ViewEngines

    type RedirectResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()


    type RenderViewResult() = 
        inherit ActionResult()

        let mutable _viewName : string = null
        let mutable _layoutName : string = null
        
        let rec find_ve_r (viewreq, enumerator:IEnumerator<IViewEngine>, reslist:List<ViewEngineResult>) : List<ViewEngineResult> =
            if enumerator.MoveNext() then
                let item = enumerator.Current
                let res = item.ResolveView viewreq
                if (res.IsSuccessful) then
                    reslist.Clear()
                    reslist.Add res
                    reslist
                else
                    reslist.Add res
                    find_ve_r (viewreq, enumerator, reslist)
            else 
                reslist

        and find_ve viewreq (viewengines:IViewEngine seq) : ViewEngineResult = 
            use enumerator = viewengines.GetEnumerator()
            let results = find_ve_r(viewreq, enumerator, (List<ViewEngineResult>()))

            if Seq.isEmpty(results) then
                failwith "no view engines? todo: decent error msg"
            else 
                let h = Seq.head(results)
                if (h.IsSuccessful) then
                    h
                else 
                    failwith "todo: decent error msg"

        member x.ViewName 
            with get() = _viewName and set v = _viewName <- v
        member x.LayoutName 
            with get() = _layoutName and set v = _layoutName <- v

        override this.Execute(context:ActionResultContext) = 
            let viewreq = new ViewRequest ( 
                                    // AreaName = context.ControllerDescriptor.Area
                                    ViewName = this.ViewName, 
                                    LayoutName = this.LayoutName,
                                    ControllerName = context.ControllerDescriptor.Name, 
                                    ActionName = context.ActionDescriptor.Name
                                )
            let reg = context.ServiceRegistry
            let folderLayout = reg.ViewFolderLayout
            folderLayout.ProcessLocations (viewreq, context.HttpContext)
            let res = find_ve (viewreq.ViewLocations) (reg.ViewEngines)
            let view = res.View
            view.Process (context.HttpContext.Response.Output, context.HttpContext)


    type JsonResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()


    type JsResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()


    type FileResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()


    type XmlResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()


