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
//  This software is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Hosting.Mvc

    open System.Web
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Routing
    open Container

    [<System.ComponentModel.Composition.Export(typeof<IComposableHandler>)>]
    type MvcComposableHandler() = 
        inherit ComposableHandler()

        override this.ProcessRequest(context:HttpContextBase) =
            let req_container = CreateRequestContainer(context);
            context.Response.Write("hello")

    type MonoRailHandlerMediator() = 
        interface IRouteHttpHandlerMediator with
            // GetHandler : request:HttpRequest * routeData:RouteData -> IHttpHandler 
            member this.GetHandler(request:HttpRequest, routeData:RouteData) : IHttpHandler =
                MvcComposableHandler() :> IHttpHandler

