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

namespace Castle.MonoRail.Hosting.Mvc.Typed

    open System
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
    open Container

    type ViewComponentResult() =
        member this.View
            with get() = ""

    [<Interface>]
    type IViewComponent =
        abstract member Render : ViewComponentResult

    [<Export>]
    type ViewComponentExecutor()=
        let mutable _controllerProviders = Enumerable.Empty<Lazy<ControllerProvider, IComponentOrder>>()

        let select_controller_provider route ctx =
            Helpers.traverseWhileNull _controllerProviders (fun p -> p.Value.Create(route, ctx))

        let build_route_match viewComponentName : RouteMatch =
            let namedParams = Dictionary<string,string>()
            namedParams.["area"] <- "viewcomponents"
            namedParams.["controller"] <- viewComponentName
            
            new RouteMatch(Unchecked.defaultof<Route>, namedParams)
        
        let render_result (result:ViewComponentResult) : HtmlString =
            HtmlString ""

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helper.order_lazy_set v

        //  when 'tvc :> IViewComponent
        member this.Execute(viewComponentName:String, context:HttpContextBase, configurer:('tvc -> unit) ) : HtmlString = 
            let rmatch = build_route_match viewComponentName
            let prototype = select_controller_provider rmatch context

            if (prototype == null) then
                ExceptionBuilder.RaiseViewComponentNotFound()
            
            let viewComponent = prototype.Instance :?> IViewComponent

            if (configurer != null) then
                configurer(viewComponent)
            
            let result = viewComponent.Render

            render_result result