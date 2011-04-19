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

namespace Castle.MonoRail.Hosting.Mvc

    open System
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.Web
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extensibility
    open Container


    [<Export>]
    type PipelineRunner() = 
        let mutable _controllerProviders = Enumerable.Empty<Lazy<ControllerProvider, IComponentOrder>>()
        let mutable _controllerExecProviders = Enumerable.Empty<Lazy<ControllerExecutorProvider, IComponentOrder>>()

        let rec select_controller_provider_rec f (enumerator:IEnumerator<Lazy<ControllerProvider, IComponentOrder>>)  = 
            if (enumerator.MoveNext()) then
                let lazy_provider = enumerator.Current
                let provider = lazy_provider.Value
                let res = f(provider)
                if (res <> Unchecked.defaultof<_>) then
                    res
                else 
                    select_controller_provider_rec f enumerator
            else 
                Unchecked.defaultof<ControllerPrototype>

        and select_controller_provider route ctx = 
            let enumerator = _controllerProviders.GetEnumerator()
            try
                select_controller_provider_rec (fun (p:ControllerProvider) -> p.Create(route, ctx)) enumerator 
            finally 
                enumerator.Dispose()

        let rec select_exec_provider_rec f (enumerator:IEnumerator<Lazy<ControllerExecutorProvider, IComponentOrder>>)  = 
            if (enumerator.MoveNext()) then
                let lazy_provider = enumerator.Current
                let provider = lazy_provider.Value
                let res = f(provider)
                if (res <> Unchecked.defaultof<_>) then
                    res
                else 
                    select_exec_provider_rec f enumerator
            else 
                Unchecked.defaultof<ControllerExecutor>

        and select_executor_provider prototype route ctx = 
            let enumerator = _controllerExecProviders.GetEnumerator()
            try
                select_exec_provider_rec (fun (p:ControllerExecutorProvider) -> p.Create(prototype, route, ctx)) enumerator 
            finally 
                enumerator.Dispose()
                

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helpers.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerExecutorProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helpers.order_lazy_set v

        member this.Execute(route_data:RouteData, context:HttpContextBase) = 
            let prototype = select_controller_provider route_data context
            
            if (prototype = Unchecked.defaultof<_>) then
                ExceptionBuilder.RaiseControllerProviderNotFound()
            else
                let executor = select_executor_provider prototype route_data context
                
                if (prototype = Unchecked.defaultof<_>) then
                    ExceptionBuilder.RaiseControllerExecutorProviderNotFound()
                else
                    executor.Execute(prototype, route_data, context)


    [<Export(typeof<IComposableHandler>)>]
    type MvcComposableHandler() = 
        inherit ComposableHandler()

        [<DefaultValue>]
        val mutable _pipeline:PipelineRunner

        [<Import()>]
        member this.Pipeline 
            with set(value) = this._pipeline <- value 

        override this.ProcessRequest(context:HttpContextBase) =
            let req_container = CreateRequestContainer(context);
            let route_data = context.Items.["mr_route_data"] :?> Castle.MonoRail.Routing.RouteData
            
            this._pipeline.Execute(route_data,context)
            
            // context.Response.Write("hello")



    type MonoRailHandlerMediator() = 
        interface IRouteHttpHandlerMediator with
            // GetHandler : request:HttpRequest * routeData:RouteData -> IHttpHandler 
            member this.GetHandler(request:HttpRequest, routeData:RouteData) : IHttpHandler =
                MvcComposableHandler() :> IHttpHandler

