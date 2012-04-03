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

namespace Castle.MonoRail.Routing

    open System
    open System.Collections.Generic
    open System.Threading
    open System.Web
    open Internal

    type Router() = 
        inherit RouteOperations(Unchecked.defaultof<Route>)
    
        static let instance = Router()

        static member Instance = instance

        member this.TryMatch (request:RequestInfo) : RouteMatch = 
            base.InternalTryMatch request

        member this.GetRoute(name:string) : Route = 
            let parts = name.Split([|'.'|])
            let mutable target = base.Routes
            let r : Ref<Route> = ref null
            for p in parts do
                r := target.[p]
                target <- (!r).Children
            !r

