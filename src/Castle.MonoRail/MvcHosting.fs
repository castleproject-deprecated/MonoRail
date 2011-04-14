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

    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open System.Web
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Routing
    open Container


    [<System.ComponentModel.Composition.Export>]
    type PipelineRunner() = 
        
        [<DefaultValue>]
        val mutable _controllerProviders:IEnumerable<ControllerProvider>
        [<DefaultValue>]
        val mutable _controllerExecProviders:IEnumerable<ControllerExecutorProvider>

        [<System.ComponentModel.Composition.Import>]
        member this.ControllerProviders
            with get() = this._controllerProviders and set(v) = this._controllerProviders <- v

        [<System.ComponentModel.Composition.Import>]
        member this.ControllerExecutorProviders
            with get() = this._controllerProviders and set(v) = this._controllerProviders <- v



    [<System.ComponentModel.Composition.Export(typeof<IComposableHandler>)>]
    type MvcComposableHandler() = 
        inherit ComposableHandler()

        [<DefaultValue>]
        val mutable _pipeline:PipelineRunner

        [<System.ComponentModel.Composition.Import()>]
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

