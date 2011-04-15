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

namespace Castle.MonoRail.Routing

open System
open System.Collections.Generic
open System.Threading
open System.Web
open Internal


type Router() = 
    inherit RouteOperations()
    
    let rec RecTryMatch (index, routes:List<Route>, request:IRequestInfo) : RouteData =
        
        if (index > routes.Count - 1) then
            Unchecked.defaultof<RouteData>
        else
            let route = routes.[index]
            let res, namedParams = route.TryMatch(request)
            if (res) then
                RouteData(route, namedParams)
            else 
                RecTryMatch(index + 1, routes, request)

    static let instance = Router()

    static member Instance
        with get() = instance

    member this.TryMatch(request:IRequestInfo) : RouteData = 
        RecTryMatch(0, base.InternalRoutes, request)

    member this.TryMatch(path:string) : RouteData = 
        RecTryMatch(0, base.InternalRoutes, RequestInfoAdapter(path, null, null, null))


