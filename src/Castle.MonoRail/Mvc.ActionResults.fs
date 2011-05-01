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


    type ViewResult() = 
        inherit ActionResult()

        let mutable _viewName : string = null
        let mutable _layoutName : string = null
        let mutable _model = obj()
        
        member x.ViewName  with get() = _viewName and set v = _viewName <- v
        member x.LayoutName  with get() = _layoutName and set v = _layoutName <- v
        member x.Model  with get() = _model and set v = _model <- v

        override this.Execute(context:ActionResultContext) = 
            let viewreq = new ViewRequest ( 
                                    // AreaName = context.ControllerDescriptor.Area
                                    ViewName = this.ViewName, 
                                    LayoutName = this.LayoutName,
                                    ControllerName = context.ControllerDescriptor.Name, 
                                    ActionName = context.ActionDescriptor.Name
                                )
            let reg = context.ServiceRegistry
            reg.ViewRendererService.Render(viewreq, context.HttpContext, _model)

    (*
    type ViewResult<'a>(model:'a) = 
        inherit ViewResult()

        // we should instead populate this.Model, but the compiler complains about generic scaping its scope (?!?)
        let mutable _typed = model

        member x.TypedModel  with get() = _typed and set v = _typed <- v

        override this.Execute(context:ActionResultContext) = 
            let viewreq = new ViewRequest ( 
                                    // AreaName = context.ControllerDescriptor.Area
                                    ViewName = this.ViewName, 
                                    LayoutName = this.LayoutName,
                                    ControllerName = context.ControllerDescriptor.Name, 
                                    ActionName = context.ActionDescriptor.Name
                                )
            let reg = context.ServiceRegistry
            reg.ViewRendererService.Render(viewreq, context.HttpContext, _typed)
    *)

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


