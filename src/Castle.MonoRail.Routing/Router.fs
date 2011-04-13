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


