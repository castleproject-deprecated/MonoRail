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


    [<AbstractClass;AllowNullLiteral>]
    type ActionSelector() = 
        abstract Select : actions:IEnumerable<ControllerActionDescriptor> * context:HttpContextBase -> ControllerActionDescriptor


    [<Export;AllowNullLiteral>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionResultExecutor [<ImportingConstructor>] (reg:IServiceRegistry) = 
        member this.Execute(result:ActionResult, action, prototype, route_match, httpctx:HttpContextBase) = 
            let ctx = ActionResultContext(action, prototype, httpctx, route_match, reg)
            result.Execute(ctx)
            

    [<AllowNullLiteral>] 
    type ActionExecutionContext
        (action:ControllerActionDescriptor, prototype:ControllerPrototype, reqCtx:HttpContextBase, routeMatch:RouteMatch) = 
        let mutable _result : obj = null
        let mutable _exception : Exception = null
        let mutable _exceptionHandled = false
        let _parameters = lazy 
                                let dict = Dictionary<string,obj>() 
                                // wondering if this isn't just a waste of cycles. 
                                // need to perf test
                                for pair in action.Parameters do
                                    dict.[pair.Name] <- null
                                dict
        
        member x.RouteMatch = routeMatch
        member x.Prototype = prototype
        member x.HttpContext = reqCtx
        member x.ControllerDescriptor = action.ControllerDescriptor
        member x.ActionDescriptor = action
        member x.Parameters = _parameters.Force()
        member x.Result 
            with get() = _result and set(v) = _result <- v
        member x.Exception
            with get() = _exception and set(v) = _exception <- v
        member x.ExceptionHandled
            with get() = _exceptionHandled and set(v) = _exceptionHandled <- v


    [<AbstractClass>]
    [<AllowNullLiteral>]
    type ActionProcessor() = 
        let _next : Ref<ActionProcessor> = ref null

        member x.ProcessNext(context:ActionExecutionContext) = 
            if !_next <> null then (!_next).Process(context)

        abstract Process : context:ActionExecutionContext -> unit

        member x.Next with get() = !_next and set v = _next := v





