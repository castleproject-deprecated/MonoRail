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
        
    
    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ActionParameterBinder)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionParameterBinderProcessor() = 
        inherit BaseActionProcessor()
        let mutable _valueProviders = Unchecked.defaultof<Lazy<IParameterValueProvider,IComponentOrder> seq>

        [<ImportMany(AllowRecomposition=true)>]
        member x.ValueProviders 
            with get() = _valueProviders and set v = _valueProviders <- Helper.order_lazy_set v

        override x.Process(context:ActionExecutionContext) = 
            // uses the IParameterValueProvider to fill parameters for the actions
            let pairs = List<KeyValuePair<string,obj>>()
            // TODO: Refactor to use high order set functions
            for p in context.Parameters do
                if p.Value = null then // if <> null, then a previous processor filled the value
                    let name = p.Key
                    let pdesc = context.ActionDescriptor.ParametersByName.[name]
                    let res, value = 
                        Helpers.traverseWhile _valueProviders 
                                              (fun vp -> vp.Value.TryGetValue(name, pdesc.ParamType) )
                    if res then 
                        pairs.Add (KeyValuePair (p.Key, value))
            
            for pair in pairs do 
                context.Parameters.[pair.Key] <- pair.Value

            x.NextProcess(context)


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ActionExecutorProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ActionExecutorProcessor () =
        inherit BaseActionProcessor()

        override x.Process(context:ActionExecutionContext) = 
            // TODO: Refactor to not use seq
            let parameters = 
                seq { 
                    for p in context.ActionDescriptor.Parameters do
                        yield context.Parameters.[p.Name]
                    }
                |> Seq.toArray

            let mutable processNext = true

            try
                context.Result <- context.ActionDescriptor.Execute(context.Prototype.Instance, parameters)
            with
            | ex -> 
                context.Exception <- ex

                processNext <- false
                x.NextProcess(context)

                if not (context.ExceptionHandled) then
                  reraise()
            
            if processNext then
                x.NextProcess(context)
            


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ActionResultExecutorProcessor)>]
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
                    _actionResultExecutor.Execute(ar, context.ActionDescriptor, 
                                                  context.ControllerDescriptor, context.Prototype, 
                                                  context.RouteMatch, context.HttpContext)
                | _ -> 
                    // we shouldnt really ignore, instead, do a default kind of action - rendering a view?
                    ignore()

            x.NextProcess(context)
