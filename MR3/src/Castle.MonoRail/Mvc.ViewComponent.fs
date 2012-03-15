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

namespace Castle.MonoRail

    open System
    open System.IO
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Castle.MonoRail.ViewEngines

    type ViewComponentResult() =
        let mutable _viewName : string = null
        let mutable _model = null
        let mutable _bag : IDictionary<string, obj> = upcast PropertyBag()

        member this.ViewName 
            with get() = _viewName and set(v) = _viewName <- v

        member this.Model
            with get() = _model and set(v) = _model <- v

        member this.Bag 
            with get() = _bag and set(v) = _bag <- v

    [<Interface>]
    type IViewComponent =
        abstract member Render : unit -> ViewComponentResult

    [<Export>]
    type ViewComponentExecutor() =
        let mutable _controllerProviders : Lazy<ControllerProvider, IComponentOrder> seq = Seq.empty
        let mutable _viewRendererSvc = Unchecked.defaultof<ViewRendererService>

        let select_controller_provider route ctx =
            Helpers.traverseWhileNull _controllerProviders (fun p -> p.Value.Create(route, ctx))

        let build_route_match (viewComponentName : String) : RouteMatch =
            let namedParams = Dictionary<string,string>()
            namedParams.["area"] <- "viewcomponents"
            namedParams.["controller"] <- viewComponentName.Replace ("Controller", "")
            
            new RouteMatch(Unchecked.defaultof<Route>, namedParams)
        
        let render_result (result:ViewComponentResult) (rmatch:RouteMatch) (context:HttpContextBase) =
            let viewreq = ViewRequest (
                            ViewName = result.ViewName, ViewFolder = rmatch.RouteParams.["controller"], DefaultName = "default", GroupFolder = "/viewcomponents"
                            )

            use output = new StringWriter()
            _viewRendererSvc.RenderPartial (viewreq, context, result.Bag, result.Model, output)
            HtmlString (output.ToString())

        [<Import>]
        member this.ViewRendererService
            with get () = _viewRendererSvc and set(v) = _viewRendererSvc <- v 

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helper.order_lazy_set v

        member this.Execute<'tvc when 'tvc :> IViewComponent>(viewComponentName:String, context:HttpContextBase, configurer:Action<'tvc>) = 
            let rmatch = build_route_match viewComponentName
            let prototype = select_controller_provider rmatch context

            if prototype = null then
                ExceptionBuilder.RaiseViewComponentNotFound()
            
            let viewComponent = prototype.Instance :?> IViewComponent

            if configurer <> null then
                configurer.Invoke(viewComponent :?> 'tvc)
            
            let result = viewComponent.Render()

            render_result result rmatch context