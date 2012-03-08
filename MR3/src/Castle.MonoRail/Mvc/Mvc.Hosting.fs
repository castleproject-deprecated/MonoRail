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
    open Castle.MonoRail.Hosting

    [<Export; AllowNullLiteral>]
    type PipelineRunner() = 
        let mutable _controllerProviders = Enumerable.Empty<Lazy<ControllerProvider, IComponentOrder>>()
        let mutable _controllerExecProviders = Enumerable.Empty<Lazy<ControllerExecutorProvider, IComponentOrder>>()

        let try_create route ctx =
            let try_create_controller (p:Lazy<ControllerProvider, IComponentOrder>) = 
                let controller = p.Value.Create(route, ctx)
                if controller <> null then Some(controller) else None
            match _controllerProviders |> Seq.tryPick try_create_controller with
            | Some controller -> controller
            | None -> null
            
        let select_executor_provider prototype route ctx = 
            let try_create_executor (p:Lazy<ControllerExecutorProvider, IComponentOrder>) = 
                let executor = p.Value.Create(prototype, route, ctx)
                if executor <> null then Some(executor) else None
            match _controllerExecProviders |> Seq.tryPick try_create_executor with
            | Some executor -> executor
            | None -> null
                
        do
            System.Diagnostics.Debug.WriteLine "PipelineRunner()"
            ()

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helper.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerExecutorProviders
            with get() = _controllerExecProviders and set(v) = _controllerExecProviders <- Helper.order_lazy_set v

        member this.TryExecute(route_data:RouteMatch, context:HttpContextBase) = 
            let prototype = try_create route_data context
            
            if prototype = null then
                // context.AddError( ExceptionBuilder.ControllerProviderNotFound() )
                false
            else
                let executor = select_executor_provider prototype route_data context
                
                if executor = null then
                    // context.AddError( ExceptionBuilder.ControllerExecutorProviderNotFound() )
                    false
                else
                    executor.Execute(prototype, route_data, context)
                    true


    [<AllowNullLiteral>]
    [<Castle.Extensibility.BundleExport(typeof<IComposableHandler>)>]
    type MvcComposableHandler() = 
        inherit ComposableHandler()

        let mutable _pipeline = Unchecked.defaultof<PipelineRunner>

        [<Import>]
        member this.Pipeline with set(v) = _pipeline <- v

        override this.TryProcessRequest(context:HttpContextBase) =
            context.Response.Buffer <- true
            let route_data = context.Items.[Constants.MR_Routing_Key] :?> Castle.MonoRail.Routing.RouteMatch
            _pipeline.TryExecute (route_data, context)


    
    type MonoRailHandlerMediator() = 
        interface IRouteHttpHandlerMediator with
            member this.GetHandler(request:HttpRequest, routeData:RouteMatch) : IHttpHandler =
                MonoRailHandler() :> IHttpHandler
    
