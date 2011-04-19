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

        let rec select_controller_provider enumerator route ctx = 
            if (enumerator.MoveNext()) then
                let provider = enumerator.Current
                try
                    let res = provider.Create (route_data, ctx)
                    
                with
                    | e -> enumerator.Dispose()

                // select_controller_provider
            else 
                enumerator.Dispose()
                Unchecked.defaultof<_>
                

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helpers.order_lazy_set v

        [<ImportMany(AllowRecomposition=true)>]
        member this.ControllerExecutorProviders
            with get() = _controllerProviders and set(v) = _controllerProviders <- Helpers.order_lazy_set v

        member this.Execute() = 
            select_controller_provider _controllerProviders.GetEnumerator()
            ignore()


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
            context.Response.Write("hello")



    type MonoRailHandlerMediator() = 
        interface IRouteHttpHandlerMediator with
            // GetHandler : request:HttpRequest * routeData:RouteData -> IHttpHandler 
            member this.GetHandler(request:HttpRequest, routeData:RouteData) : IHttpHandler =
                MvcComposableHandler() :> IHttpHandler

