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

namespace Castle.MonoRail.Hosting.Mvc

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
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Container

    [<Export>]
    type PipelineRunner() = 
        let mutable _controllerProviders = Enumerable.Empty<Lazy<ControllerProvider, IComponentOrder>>()
        let mutable _controllerExecProviders = Enumerable.Empty<Lazy<ControllerExecutorProvider, IComponentOrder>>()

        let select_controller_provider route ctx =
            Helpers.traverseWhileNull _controllerProviders (fun p -> p.Value.Create(route, ctx))
            
        let select_executor_provider prototype route ctx = 
            Helpers.traverseWhileNull _controllerExecProviders (fun p -> p.Value.Create(prototype, route, ctx))
                
        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helper.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerExecutorProviders
            with get() = _controllerExecProviders and set(v) = _controllerExecProviders <- Helper.order_lazy_set v

        member this.Execute(route_data:RouteMatch, context:HttpContextBase) = 
            let prototype = select_controller_provider route_data context
            
            if (prototype = Unchecked.defaultof<_>) then
                ExceptionBuilder.RaiseControllerProviderNotFound()
            else
                let executor = select_executor_provider prototype route_data context
                
                if (executor = Unchecked.defaultof<_>) then
                    ExceptionBuilder.RaiseControllerExecutorProviderNotFound()
                else
                    executor.Execute(prototype, route_data, context)


    [<Export(typeof<IComposableHandler>)>]
    type MvcComposableHandler() = 
        inherit ComposableHandler()

        let mutable _pipeline = Unchecked.defaultof<PipelineRunner>

        [<Import>]
        member this.Pipeline 
            with set(v) = _pipeline <- v

        override this.ProcessRequest(context:HttpContextBase) =
            let route_data = context.Items.[Constants.MR_Routing_Key] :?> Castle.MonoRail.Routing.RouteMatch
            _pipeline.Execute(route_data,context)


    type MonoRailHandlerMediator() = 
        interface IRouteHttpHandlerMediator with
            member this.GetHandler(request:HttpRequest, routeData:RouteMatch) : IHttpHandler =
                MvcComposableHandler() :> IHttpHandler

