namespace Castle.MonoRail.Routing

open System
open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading
open System.Web
open Internal


[<AbstractClass>]
type RouteOperations() = 
    let _routes = ConcurrentBag<Route>()

    member this.Routes 
        with get() : IEnumerable<Route> = _routes :> IEnumerable<Route>

    member this.Match(path:string)  = 
        Assertions.ArgNotNullOrEmpty path

        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, null, path, None)
        _routes.Add(route)
        route

    member this.Match(path:string, name:string) = 
        Assertions.ArgNotNullOrEmpty path
        Assertions.ArgNotNullOrEmpty name
        
        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, name, path, None)
        _routes.Add(route)
        route

    member this.Match(path:string, config:Action<RouteConfig>) = 
        Assertions.ArgNotNullOrEmpty path
        Assertions.ArgNotNull config

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg))
        _routes.Add(route)
        route

    member this.Match(path:string, name:string, config:Action<RouteConfig>) = 
        Assertions.ArgNotNullOrEmpty path
        Assertions.ArgNotNullOrEmpty name
        Assertions.ArgNotNull config

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg))
        _routes.Add(route)
        route

    member this.Resource(name:string)  = 
        Assertions.ArgNotNullOrEmpty name

    member this.Resources(name:string)  = 
        Assertions.ArgNotNullOrEmpty name
        // generate parent route for name with the following children
        //   new
        //   create
        //   edit
        //   update
        //   view
        //   delete

    member this.Resources(name:string, identifier:string)  = 
        Assertions.ArgNotNullOrEmpty name
        Assertions.ArgNotNullOrEmpty identifier



and Route internal (routeNodes, name, path, config:Option<RouteConfig>) = 
    let _routeNodes = routeNodes;
    let _name = name
    let _path = path
    let _config = config
    let mutable _action:Action<HttpRequestBase, HttpResponseBase> = null

    member this.Action 
        with get() = _action
        and set(value) = _action <- value


    member this.Redirect(url:string) = 
        Assertions.NotImplemented()
        ignore


    member this.PermanentRedirect(url:string) = 
        Assertions.NotImplemented()
        ignore


    member this.Name 
        with get() = _name


    member this.Path 
        with get() = _path


    member this.RouteConfig 
        with get() : RouteConfig = 
            match _config with 
                | Some(_) -> _config.Value
                | _ -> Unchecked.defaultof<_>;


    member this.Generate() = 
        ignore()


    member internal this.TryMatch(path:string, protocol:string, domain:string, httpMethod:string) = 

        let matchReqs = TryMatchRequirements(protocol, domain, httpMethod)
        let mutable namedParams = Map<string,string>([])
        
        if matchReqs = false then
            false, namedParams
        else
            let res, index = RecursiveMatch(path, 0, 0, _routeNodes, &namedParams)
            res, namedParams


    member internal this.RouteNodes
        with get() = _routeNodes


and RouteConfig() =
    inherit RouteOperations()

    let mutable _controller:string = null
    let mutable _domain:string = null
    let mutable _method:string = null
    let mutable _protocol:string = null
    let mutable _action:string = null

    member this.Protocol(protocol:string) = 
        _protocol <- protocol;
        this

    member this.Domain(domain:string) = 
        _domain <- domain;
        this

    member this.HttpMethod(verb:string) = 
        _method <- verb;
        this

    member this.Controller(name:string) : RouteConfig =
        _controller <- name
        this

    member this.Controller<'T>() : RouteConfig =
        _controller <- typeof<'T>.Name // need to be reviewed
        this

    member this.Action(name:string) : RouteConfig =
        _controller <- name
        this

    member this.Param(name:string) : ParamConfig = 
        ParamConfig(this)


and ParamConfig(config) = 
    let _routeConfig = config
    
    member this.Decimal() = 
        Assertions.NotImplemented()
        this

    member this.Config() = 
        _routeConfig


and RouteData internal (route:Route, namedParams:Map<string,string>) = 
    let _route = route
    let _namedParams = namedParams

    member this.Route 
        with get() = _route

    member this.RouteParams 
        with get() : IDictionary<string,string> = _namedParams :> IDictionary<string,string>



type Router() = 
    inherit RouteOperations()
    static let instance = Router()

    static member Instance
        with get() = instance

    member this.TryMatch(path:string, protocol:string, domain:string, httpMethod:string) = 
        // for route in base.Routes do
        let routes = base.Routes
        let s = seq { for r in routes do yield r }
        let selnamedParams = ref (Map<string,string>([]))


        let matching (route:Route) : bool = 
            let res, namedParams = route.TryMatch(path, protocol, domain, httpMethod)
            selnamedParams := namedParams
            res

        try
            let found = s |> Seq.find matching 
            RouteData(found, !selnamedParams)
        with 
            | :? KeyNotFoundException -> Unchecked.defaultof<_>



