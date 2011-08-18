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

namespace Castle.MonoRail.Hosting.Mvc.Typed

    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open System.Runtime.InteropServices

    [<AbstractClass>]
    type ActionSelector() = 
        abstract Select : actions:IEnumerable<ControllerActionDescriptor> * context:HttpContextBase -> ControllerActionDescriptor


    [<Export>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionResultExecutor [<ImportingConstructor>] (reg:IServiceRegistry) = 
        let _registry = reg
        
        member this.Execute(result:ActionResult, action, controller, prototype, route_match, httpctx:HttpContextBase) = 
            let ctx = ActionResultContext(action, controller, prototype, httpctx, route_match, _registry)
            result.Execute(ctx)
            ignore()

        
    type ActionExecutionContext
        (action:ControllerActionDescriptor, controller:ControllerDescriptor, prototype:ControllerPrototype, reqCtx:HttpContextBase, routeMatch:RouteMatch) = 
        let mutable _result = Unchecked.defaultof<obj>
        let mutable _exception = Unchecked.defaultof<Exception>
        let _parameters = lazy (
                let dict = Dictionary<string,obj>() 
                // wondering if this isn't just a waste of cycles. 
                // need to perf test
                for pair in action.Parameters do
                    dict.[pair.Name] <- null
                dict
            )
        
        member x.RouteMatch = routeMatch
        member x.Prototype = prototype
        member x.HttpContext = reqCtx
        member x.ControllerDescriptor = controller
        member x.ActionDescriptor = action
        member x.Parameters = _parameters.Force()
        member x.Result 
            with get() = _result and set(v) = _result <- v
        member x.Exception
            with get() = _exception and set(v) = _exception <- v


    [<Interface>]
    type IActionProcessor = 
        abstract Next : IActionProcessor with get, set
        abstract Process : context:ActionExecutionContext -> unit


    [<AbstractClass>]
    type BaseActionProcessor() as self = 
        let mutable _next = Unchecked.defaultof<IActionProcessor>

        member x.NextProcess(context:ActionExecutionContext) = 
            if (_next != null) then
                _next.Process(context)

        abstract Process : context:ActionExecutionContext -> unit

        interface IActionProcessor with
            member x.Next
                with get() = _next and set v = _next <- v
            member x.Process(context:ActionExecutionContext) = self.Process(context)