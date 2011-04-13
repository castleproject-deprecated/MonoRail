namespace Castle.MonoRail.Routing

open System
open System.Collections.Generic
open System.Threading
open System.Web
open Internal


[<Interface>]
type IRequestInfo = 
    abstract Path : string
    abstract Protocol : string
    abstract HttpMethod : string
    abstract Domain : string


  

type RequestInfoAdapter(path:string, protocol:string, httpMethod:string, domain:string) = 
    let _path = path
    let _protocol = protocol
    let _method = httpMethod
    let _domain = domain

    new (request:HttpRequestBase) =
        RequestInfoAdapter(request.Path, request.Url.Scheme, request.HttpMethod, request.Url.Host)
    new (request:HttpRequest) =
        RequestInfoAdapter(request.Path, request.Url.Scheme, request.HttpMethod, request.Url.Host)

    interface IRequestInfo with
        member this.Path 
            with get() = _path
        member this.Protocol 
            with get() = _protocol
        member this.HttpMethod 
            with get() = _method
        member this.Domain 
            with get() = _domain


[<AbstractClass>]
type RouteOperations() = 
    let _routes = List<Route>()

    member internal this.InternalRoutes 
        with get() = _routes

    member this.Routes 
        with get() : IEnumerable<Route> = _routes :> IEnumerable<Route>

    member this.Match(path:string)  = 
        Assertions.ArgNotNullOrEmpty (path, "path")

        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, null, path, None)
        _routes.Add(route)
        route

    member this.Match(path:string, name:string) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        
        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, name, path, None)
        _routes.Add(route)
        route

    member this.Match(path:string, config:Action<RouteConfig>) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNull (config, "config")

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg))
        _routes.Add(route)
        route

    member this.Match(path:string, name:string, config:Action<RouteConfig>) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull (config, "config")

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg))
        _routes.Add(route)
        route

    member this.Resource(name:string)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")

    member this.Resources(name:string)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        // generate parent route for name with the following children
        //   new
        //   create
        //   edit
        //   update
        //   view
        //   delete

    member this.Resources(name:string, identifier:string)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNullOrEmpty (identifier, "identifier")



and Route internal (routeNodes, name, path, config:Option<RouteConfig>) = 
    let _routeNodes = routeNodes;
    let _name = name
    let _path = path
    let _config = config
    let mutable _handler:IRouteHttpHandlerMediator = Unchecked.defaultof<_>
    let mutable _action:Action<HttpRequestBase, HttpResponseBase> = null

    let TryMatchRequirements(request:IRequestInfo) = 
        true

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

    member this.HandlerMediator
        with get() = _handler and set(value) = _handler <- value

    member this.Generate() = 
        ignore()

    member internal this.TryMatch(request:IRequestInfo) = 
        let matchReqs = TryMatchRequirements(request)
        let mutable namedParams = Dictionary<string,string>()
        
        if matchReqs = false then
            false, namedParams
        else
            let path = request.Path
            let res, index = RecursiveMatch(path, 0, 0, _routeNodes, namedParams)
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


and RouteData internal (route:Route, namedParams:IDictionary<string,string>) = 
    let _route = route
    let _namedParams = namedParams

    member this.Route 
        with get() = _route

    member this.RouteParams 
        with get() : IDictionary<string,string> = _namedParams

// [<Interface>]
and IRouteHttpHandlerMediator = 
    abstract GetHandler : HttpRequest * RouteData -> IHttpHandler 


open System.Runtime.Serialization

[<Serializable>]
type RouteException = 
    inherit Exception
    new (msg) = { inherit Exception(msg) }
    new (info:SerializationInfo, context:StreamingContext) = 
        { 
            inherit Exception(info, context)
        }
