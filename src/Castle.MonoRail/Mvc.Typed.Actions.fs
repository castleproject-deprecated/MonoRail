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
    open Helpers


    [<AbstractClass>]
    type ActionSelector() = 
        abstract Select : actions:IEnumerable<ControllerActionDescriptor> * context:HttpContextBase -> ControllerActionDescriptor


    [<Export>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionResultExecutor [<ImportingConstructor>] (reg:IServiceRegistry) = 
        let _registry = reg
        member this.Execute(ar:ActionResult, request:HttpContextBase) = 
            ar.Execute(request, _registry)


    [<Interface>]
    type IParameterValueProvider = 
        //   Routing, (Forms, QS, Cookies), Binder?, FxValues?
        abstract TryGetValue : name:string * paramType:Type * value:obj byref -> bool
    

    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 10000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type RoutingValueProvider [<ImportingConstructor>] (route_match:RouteMatch) = 
        let _route_match = route_match

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 
                value <- null
                false


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type RequestBoundValueProvider [<ImportingConstructor>] (request:HttpRequestBase) = 
        let _request = request

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 
                value <- null
                // if (paramType.IsPrimitive) then 
                    // _request.Params.[name]
                    // false
                false
        

    type ActionExecutionContext
        (action:ControllerActionDescriptor, controller:ControllerDescriptor, instance, reqCtx) = 
        let _action = action
        let _controller = controller
        let _instance = instance
        let _reqCtx = reqCtx
        let mutable _result = Unchecked.defaultof<obj>
        let mutable _exception = Unchecked.defaultof<Exception>
        let _parameters = lazy ( 
                let dict = Dictionary<string,obj>() 
                // wondering if this isn't just a waste of cycles. 
                // need to perf test
                for pair in _action.Parameters do
                    dict.[pair.Name] <- null
                dict
            )
        
        member x.HttpContext = _reqCtx
        member x.Instance = _instance
        member x.Controller = _controller
        member x.Action = _action
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


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", 10000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionParameterBinderProcessor() = 
        inherit BaseActionProcessor()
        let mutable _valueProviders = Unchecked.defaultof<Lazy<IParameterValueProvider,IComponentOrder> seq>

        [<ImportMany(AllowRecomposition=true)>]
        member x.ValueProviders 
            with get() = _valueProviders and set v = _valueProviders <- Helper.order_lazy_set v

        override x.Process(context:ActionExecutionContext) = 
            // let copy = Dictionary<string,obj>(context.Parameters)
            // uses the IParameterValueProvider to fill parameters for the actions
            for p in context.Parameters do
                if p.Value = null then // if <> null, then a previous processor filled the value
                    let name = p.Key
                    let pdesc = context.Action.ParametersByName.[name]
                    for vp in _valueProviders do
                        let tempVal = obj()
                        let res = vp.Value.TryGetValue(name, pdesc.ParamType, ref tempVal)
                        if (res) then
                            context.Parameters.[p.Key] <- tempVal

            x.NextProcess(context)
   
    
    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", 100000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionExecutorProcessor() = 
        inherit BaseActionProcessor()

        override x.Process(context:ActionExecutionContext) = 
            let parameters = 
                seq { 
                    for p in context.Action.Parameters do
                        yield context.Parameters.[p.Name]
                    }
                |> Seq.toArray

            try
                context.Result <- context.Action.Execute(context.Instance, parameters)
            with
            | ex -> context.Exception <- ex

            x.NextProcess(context)


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", 110000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type InvocationErrorProcessorProcessor() = 
        inherit BaseActionProcessor()

        override x.Process(context:ActionExecutionContext) = 
            if (context.Exception != null) then 
                raise context.Exception

            x.NextProcess(context)


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", 1000000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionResultExecutorProcessor 
        [<ImportingConstructor>] (arExecutor:ActionResultExecutor) = 
        inherit BaseActionProcessor()

        let _actionResultExecutor = arExecutor

        override x.Process(context:ActionExecutionContext) = 
            if (context.Exception == null) then
                let res = context.Result

                match res with 
                | :? ActionResult as ar -> 
                    _actionResultExecutor.Execute(ar, context.HttpContext)
                | _ -> ignore()

            x.NextProcess(context)

