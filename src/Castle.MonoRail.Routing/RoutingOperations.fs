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

    member this.Match(path:string, handlerMediator:IRouteHttpHandlerMediator)  = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")

        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, null, path, None, handlerMediator)
        _routes.Add(route)
        route

    member this.Match(path:string, name:string, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        
        let routeNode = parseRoutePath(path)
        let route = new Route(routeNode, name, path, None, handlerMediator)
        _routes.Add(route)
        route

    member this.Match(path:string, config:Action<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNull (config, "config")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg), handlerMediator)
        _routes.Add(route)
        route

    member this.Match(path:string, name:string, config:Action<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull (config, "config")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")

        let routeNode = parseRoutePath(path)
        let cfg = RouteConfig()
        config.Invoke(cfg)
        let route = new Route(routeNode, null, path, Some(cfg), handlerMediator)
        _routes.Add(route)
        route

    member this.Resource(name:string, handlerMediator:IRouteHttpHandlerMediator)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        ExceptionBuilder.RaiseNotImplemented()

    member this.Resources(name:string, handlerMediator:IRouteHttpHandlerMediator)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        ExceptionBuilder.RaiseNotImplemented()
        // generate parent route for name with the following children
        //   new
        //   create
        //   edit
        //   update
        //   view
        //   delete

    member this.Resources(name:string, identifier:string, handlerMediator:IRouteHttpHandlerMediator)  = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNullOrEmpty (identifier, "identifier")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        ExceptionBuilder.RaiseNotImplemented()


and Route internal (routeNodes, name, path, config:Option<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
    let _routeNodes = routeNodes;
    let _name = name
    let _path = path
    let _config = config
    let _handler:IRouteHttpHandlerMediator = handlerMediator
    let mutable _action:Action<HttpRequestBase, HttpResponseBase> = null

    let TryMatchRequirements(request:IRequestInfo) = 
        if (_config = None) then
            true
        else
            _config.Value.TryMatchRequirements(request)

    member this.Action 
        with get() = _action
        and set(value) = _action <- value

    member this.Redirect(url:string) = 
        ExceptionBuilder.RaiseNotImplemented()
        ignore

    member this.PermanentRedirect(url:string) = 
        ExceptionBuilder.RaiseNotImplemented()
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
        with get() = _handler 

    member this.Generate() = 
        ExceptionBuilder.RaiseNotImplemented()
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

    // member this.Defaults( another lambda ) : RouteConfig

    member internal this.TryMatchRequirements(request:IRequestInfo) = 
        if ((_method <> null) && (String.Compare(request.HttpMethod, _method, true) <> 0)) then
            false
        elif ((_protocol <> null) && (String.Compare(request.Protocol, _protocol, true) <> 0)) then
            false
        elif ((_domain <> null) && (String.Compare(request.Domain, _domain, true) <> 0)) then
            false
        else
            true


and ParamConfig(config) = 
    let _routeConfig = config
    
    member this.Decimal() = 
        ExceptionBuilder.RaiseNotImplemented()
        this

    member this.Config() = 
        _routeConfig


and RouteMatch internal (route:Route, namedParams:IDictionary<string,string>) = 
    let _route = route
    let _namedParams = namedParams

    new() = 
        RouteMatch(Unchecked.defaultof<Route>, Unchecked.defaultof<IDictionary<string,string>>)

    member this.Route 
        with get() = _route

    member this.RouteParams 
        with get() : IDictionary<string,string> = _namedParams

and [<Interface>] IRouteHttpHandlerMediator = 
    abstract GetHandler : request:HttpRequest * routeData:RouteMatch -> IHttpHandler 


open System.Runtime.Serialization

[<Serializable>]
type RouteException = 
    inherit Exception
    new (msg) = { inherit Exception(msg) }
    new (info:SerializationInfo, context:StreamingContext) = 
        { 
            inherit Exception(info, context)
        }
