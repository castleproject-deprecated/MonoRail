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
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extensibility
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Helpers

    [<AbstractClass>]
    type ActionSelector() = 
        abstract Select : actions:IEnumerable<ControllerActionDescriptor> * context:HttpContextBase -> ControllerActionDescriptor


    [<Export>]
    type ActionResultExecutor [<ImportingConstructor>] (reg:IServiceRegistry) = 
        let _registry = reg
        member this.Execute(ar:ActionResult, request:HttpContextBase) = 
            ar.Execute(request, _registry)


    [<Export(typeof<ActionSelector>)>]
    type DefaultActionSelector() = 
        inherit ActionSelector()

        let rec select_action (enumerator:IEnumerator<ControllerActionDescriptor>) (context:HttpContextBase) = 
            if (enumerator.MoveNext()) then
                let action = enumerator.Current
                if action.SatisfyRequest(context) then
                    action
                else
                    select_action enumerator context
            else
                Unchecked.defaultof<_>

        override this.Select(actions:IEnumerable<ControllerActionDescriptor>, context:HttpContextBase) = 
            let enumerator = actions.GetEnumerator()
            try
                select_action enumerator context
            finally 
                enumerator.Dispose()

    
    [<ControllerExecutorProviderExport(9000000)>]
    type PocoControllerExecutorProvider() = 
        inherit ControllerExecutorProvider()
        let mutable _execFactory = Unchecked.defaultof<ExportFactory<PocoControllerExecutor>>

        [<Import>]
        member this.ExecutorFactory
            with get() = _execFactory and set(v) = _execFactory <- v

        override this.Create(prototype:ControllerPrototype, data:RouteMatch, context:HttpContextBase) = 
            match prototype with
            | :? TypedControllerPrototype as inst_prototype ->
                let executor = _execFactory.CreateExport().Value
                // executor.Prototype <- inst_prototype
                executor :> ControllerExecutor
            | _ -> 
                Unchecked.defaultof<ControllerExecutor>
        
    and 
        [<Export>] 
        PocoControllerExecutor 
            [<ImportingConstructor>] 
            (arExecutor:ActionResultExecutor, 
             [<ImportMany(RequiredCreationPolicy=CreationPolicy.NonShared)>] actionMsgs:Lazy<IActionProcessor, IComponentOrder> seq) = 
            inherit ControllerExecutor()
            
            let _actionResultExecutor = arExecutor
            let _actionMsgs = Helper.order_lazy_set actionMsgs
            let mutable _actionSelector = Unchecked.defaultof<ActionSelector>
            
            let prepare_msgs (msgs:Lazy<IActionProcessor, IComponentOrder> seq) = 
                let mutable prev = Unchecked.defaultof<Lazy<IActionProcessor, IComponentOrder>>
                let mutable first = Unchecked.defaultof<IActionProcessor>

                for msg in msgs do
                    if first == null then 
                        first <- msg.Value
                    if prev <> null then
                       prev.Value.Next <- msg.Value
                    prev <- msg

                first

            [<Import>]
            member this.ActionSelector
                with get() = _actionSelector and set(v) = _actionSelector <- v

            override this.Execute(controller:ControllerPrototype, route_data:RouteMatch, context:HttpContextBase) = 
                let action_name = route_data.RouteParams.["action"]
                let prototype = controller :?> TypedControllerPrototype
                let desc = prototype.Descriptor
                
                let candidates = 
                    desc.Actions.Where (fun (can:ControllerActionDescriptor) -> String.Compare(can.Name, action_name, StringComparison.OrdinalIgnoreCase) = 0)
                    
                if (not (candidates.Any())) then
                    ExceptionBuilder.RaiseMRException(ExceptionBuilder.CandidatesNotFoundMsg(action_name))
                
                let action = _actionSelector.Select (candidates, context)
                if (action = Unchecked.defaultof<_>) then
                    ExceptionBuilder.RaiseMRException(ExceptionBuilder.CandidatesNotFoundMsg(action_name))

                let firstMsg = prepare_msgs _actionMsgs

                if (firstMsg == null) then
                    ExceptionBuilder.RaiseMRException(ExceptionBuilder.EmptyActionProcessors)
                
                let ctx = ActionExecutionContext(action, desc)
                firstMsg.Process ctx 

                (*
                let result = action.Execute(prototype.Instance, [||])

                match result with 
                | :? ActionResult as ar -> 
                    _actionResultExecutor.Execute(ar, context)
                | _ -> 
                    // temporary
                    context.Response.Write("Action did not return anything") // nothing to do? render? what?
                *)

                ignore()


