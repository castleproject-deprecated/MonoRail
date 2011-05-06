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
open System.Text
open System.Web
open Internal
open Helpers

type RouteCollection(routes:IList<Route>) = 
    inherit System.Collections.ObjectModel.ReadOnlyCollection<Route>(routes)
    let _dict = lazy ( 
                    let d = Dictionary<string,Route>()
                    for r in routes do
                        if r.Name != null then
                            d.[r.Name] <- r 
                    d
                )
    member x.Item
        with get(name:string) = _dict.Force().[name]


and [<AbstractClass>] 
    RouteOperations(parent:Route) = 
    let _parent = parent
    let _routes = List<Route>()

    let merge (dict1:IDictionary<string,string>) (dict2:IDictionary<string,string>) =
        for pair in dict2 do
            dict1.[pair.Key] <- pair.Value

    let try_match (route:Route) (request:IRequestInfo) =
        let matchReqs, hasChildren = 
            if route.HasConfig then 
                let cfg = (route.RouteConfig :> RouteConfig)
                cfg.TryMatchRequirements request, cfg.HasChildren
            else
                true, false
        
        if matchReqs = false then
            false, null, 0
        else
            let path = request.Path
            let namedParams = Dictionary<string,string>()
            let res, index = RecursiveMatch path request.PathStartIndex 0 route.RouteNodes namedParams route.DefaultValues hasChildren
            if (res) then
                true, namedParams, index
            else
                false, null, 0

    let rec rec_try_match index (routes:List<Route>) (request:IRequestInfo) : RouteMatch =
        if (index > routes.Count - 1) then
            Unchecked.defaultof<RouteMatch>
        else
            let route = routes.[index]
            let res, namedParams, newindex = try_match route request
            if (res) then
                let haschildren = 
                    if route.HasConfig then (route.RouteConfig :> RouteConfig).HasChildren else false
                if (haschildren) then
                    request.PathStartIndex <- newindex
                    let ops = route.RouteConfig :> RouteOperations
                    let nodes = ops.InternalRoutes
                    let innermatch = rec_try_match 0 nodes request

                    if innermatch != null then
                        merge innermatch.RouteParams namedParams
                        innermatch
                    else
                        Unchecked.defaultof<RouteMatch>
                else 
                    RouteMatch(route, namedParams)
            else 
                rec_try_match (index + 1) routes request

    member this.Routes = RouteCollection(_routes)
    member internal this.InternalRoutes = _routes

    member internal this.InternalTryMatch (request:IRequestInfo) : RouteMatch = 
        rec_try_match 0 _routes request

    member internal this.InternalTryMatch(path:string) : RouteMatch = 
        rec_try_match 0 _routes (RequestInfoAdapter(path, null, null, null))

    member this.Match(path:string, handlerMediator:IRouteHttpHandlerMediator)  = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        this.InternalMatch (path, null, null, handlerMediator)

    member this.Match(path:string, name:string, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        this.InternalMatch (path, name, null, handlerMediator)

    member this.Match(path:string, config:Action<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNull (config, "config")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        this.InternalMatch (path, null, config, handlerMediator)

    member this.Match(path:string, name:string, config:Action<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
        Assertions.ArgNotNullOrEmpty (path, "path")
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNull (config, "config")
        Assertions.ArgNotNull_ (handlerMediator, "handlerMediator")
        this.InternalMatch (path, name, config, handlerMediator)

    member internal this.InternalMatch(path:string, name:string, config:Action<RouteConfig>, handlerMediator:IRouteHttpHandlerMediator) = 
        let routeNode = parseRoutePath(path)
        let route = new Route(_parent, routeNode, name, path, handlerMediator)
        if config != null then
            let cfg = RouteConfig(route)
            config.Invoke(cfg)
            route.RouteConfig <- cfg
        _routes.Add(route)
        route

and Route internal (parent, routeNodes, name, path, handlerMediator:IRouteHttpHandlerMediator) = 
    let _parent = parent
    let _routeNodes = routeNodes
    let _name = name
    let _path = path
    let _handler = handlerMediator
    let _defValues = lazy Dictionary<string,string>()
    let mutable _config = Unchecked.defaultof<RouteConfig>
    
    // this is very order dependant, but shouldnt be a problem in practice
    let _children = lazy ( 
                            let children : IList<Route> = 
                                if (_config != null) then upcast (_config.Routes) else upcast (List<Route>())
                            RouteCollection(children) 
                         )

    let rec rec_generate_url (route:Route) (buffer:StringBuilder) (parameters:IDictionary<string,string>) = 

        if route.Parent != null then 
            rec_generate_url route.Parent buffer parameters

        let r, msg = RecursiveGenerate buffer 0 route.RouteNodes (List<string>()) parameters (route.DefaultValues)
        if not r then 
            ExceptionBuilder.RaiseRouteException msg
            

    // let mutable _action:Action<HttpRequestBase, HttpResponseBase> = null

    (*
    let TryMatchRequirements(request:IRequestInfo) = 
        if (_config == null) then
            true
        else
            _config.TryMatchRequirements(request)

    member this.Action 
        with get() = _action
        and set(value) = _action <- value
    *)

    member this.Redirect(url:string) = 
        ExceptionBuilder.RaiseNotImplemented()
        ignore

    member this.PermanentRedirect(url:string) = 
        ExceptionBuilder.RaiseNotImplemented()
        ignore

    member this.Parent = _parent
    member this.Children = _children.Force() 
    member this.Name = _name
    member this.Path = _path
    member this.RouteConfig 
        with get() = 
            if _config == null then 
                _config <- RouteConfig(this)
            _config
        and internal set(v) = _config <- v

    member this.HandlerMediator = _handler 

    member this.Generate(virtualDir:string, parameters:IDictionary<string,string>) : string = 
        Assertions.ArgNotNull_ (virtualDir, "virtualDir")
        Assertions.ArgNotNull_ (parameters, "parameters")

        let buffer = 
            if (virtualDir.EndsWith("/", StringComparison.OrdinalIgnoreCase)) then
                System.Text.StringBuilder(virtualDir.Substring(0, virtualDir.Length - 1))
            else
                System.Text.StringBuilder(virtualDir)

        rec_generate_url this buffer parameters 

        let result = buffer.ToString()
        if (result = String.Empty) then
            "/"
        else
            result


    member internal x.HasConfig = _config != null
    member internal this.DefaultValues = _defValues.Force()
    member internal this.RouteNodes = _routeNodes


and RouteConfig(route:Route) =
    inherit RouteOperations(route)
    let _route = route
    let mutable _controller:string = null
    let mutable _domain:string = null
    let mutable _method:string = null
    let mutable _protocol:string = null
    let mutable _action:string = null
    let mutable _haschildren = false

    member this.Protocol(protocol:string) = 
        _protocol <- protocol;
        this

    member this.Domain(domain:string) = 
        _domain <- domain;
        this

    member this.HttpMethod(verb:string) = 
        _method <- verb
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

    // member this.Param(name:string) : ParamConfig = 
    //    ParamConfig(this)

    member this.Defaults(configExp:Action<DefaultsConfig>) : RouteConfig = 
        let defConfig = DefaultsConfig(this)
        configExp.Invoke(defConfig)
        this

    member internal this.HasChildren = base.InternalRoutes.Count <> 0

    member internal this.DefaultValueForNamedParam name value = 
        _route.DefaultValues.[name] <- value

    member internal this.TryMatchRequirements(request:IRequestInfo) = 
        if ((_method <> null) && (String.Compare(request.HttpMethod, _method, StringComparison.OrdinalIgnoreCase) <> 0)) then
            false
        elif ((_protocol <> null) && (String.Compare(request.Protocol, _protocol, StringComparison.OrdinalIgnoreCase) <> 0)) then
            false
        elif ((_domain <> null) && (String.Compare(request.Domain, _domain, StringComparison.OrdinalIgnoreCase) <> 0)) then
            false
        else
            true

and DefaultsConfig(config) = 
    let _routeConfig = config
    
    member this.Controller(name:string) = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        _routeConfig.DefaultValueForNamedParam "controller" name
        this

    member this.Action(name:string) = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        _routeConfig.DefaultValueForNamedParam "action" name
        this

    member this.Param(name:string, value:string) = 
        Assertions.ArgNotNullOrEmpty (name, "name")
        Assertions.ArgNotNullOrEmpty (value, "value")
        _routeConfig.DefaultValueForNamedParam name value 
        this
(*
and ParamConfig(config) = 
    let _routeConfig = config
    
    member this.Decimal() = 
        ExceptionBuilder.RaiseNotImplemented()
        this

    member this.Config() = 
        _routeConfig
*)


and RouteMatch (route:Route, namedParams:IDictionary<string,string>) = 
    let _route = route
    let _namedParams = namedParams

    member this.Route = _route
    member this.RouteParams : IDictionary<string,string> = _namedParams


and [<Interface>] IRouteHttpHandlerMediator = 
    abstract GetHandler : request:HttpRequest * routeData:RouteMatch -> IHttpHandler 




