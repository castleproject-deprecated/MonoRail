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
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility


    [<Export(typeof<ActionSelector>)>]
    type DefaultActionSelector() = 
        inherit ActionSelector()

        override this.Select(actions:IEnumerable<ControllerActionDescriptor>, context:HttpContextBase) = 
            match actions |> Seq.tryFind (fun action -> action.SatisfyRequest(context)) with
            | Some selection -> selection
            | _ -> null

    
    [<ControllerExecutorProviderExport(9000000)>]
    type PocoControllerExecutorProvider() = 
        inherit ControllerExecutorProvider()
        let mutable _execFactory : ExportFactory<PocoControllerExecutor> = null

        [<Import(RequiredCreationPolicy=CreationPolicy.NewScope)>]
        member this.ExecutorFactory
            with get() = _execFactory and set(v) = _execFactory <- v

        override this.Create(prototype:ControllerPrototype, data:RouteMatch, context:HttpContextBase) = 
            match prototype with
            | :? TypedControllerPrototype as inst_prototype ->
                let exp = _execFactory.CreateExport();
                let executor = exp.Value
                executor.Lifetime <- exp
                upcast executor 
            | _ -> 
                null
        

    and [<Export>] 
        [<PartMetadata("Scope", ComponentScope.Request)>]
        PocoControllerExecutor 
            [<ImportingConstructor>] 
            ([<ImportMany(RequiredCreationPolicy=CreationPolicy.NonShared)>] actionMsgs:Lazy<IActionProcessor, IComponentOrder> seq) = 
            inherit ControllerExecutor()
            
            let _actionMsgs = Helper.order_lazy_set actionMsgs
            let mutable _actionSelector : ActionSelector = null
            let mutable _lifetime : ExportLifetimeContext<PocoControllerExecutor> = null
            
            let prepare_msgs (msgs:Lazy<IActionProcessor, IComponentOrder> seq) = 
                let prev  : Ref<Lazy<IActionProcessor, IComponentOrder>> = ref null
                let first : Ref<IActionProcessor> = ref null
                for msg in msgs do
                    if !first = null then 
                        first := msg.Value
                    if !prev <> null then
                       (!prev).Value.Next <- msg.Value
                    prev := msg
                first

            member this.Lifetime       with get() = _lifetime and set(v) = _lifetime <- v
            [<Import>]
            member this.ActionSelector with get() = _actionSelector and set(v) = _actionSelector <- v

            override this.Execute(controller:ControllerPrototype, route_data:RouteMatch, context:HttpContextBase) = 

                try
                    let action_name = route_data.RouteParams.["action"]
                    let prototype = controller :?> TypedControllerPrototype
                    let desc = prototype.Descriptor
                    
                    // get the list of actions that match the request action name
                    let candidates = desc.Actions |> Seq.filter (fun (can:ControllerActionDescriptor) -> can.IsMatch action_name)
                    if Seq.isEmpty candidates then ExceptionBuilder.RaiseMRException(ExceptionBuilder.CandidatesNotFoundMsg(action_name))
                    
                    // reduce the list to one
                    let action = _actionSelector.Select (candidates, context)
                    if action = null then ExceptionBuilder.RaiseMRException(ExceptionBuilder.CandidatesNotFoundMsg(action_name))

                    // order and connect the action processors
                    let firstMsg = prepare_msgs _actionMsgs
                    if !firstMsg = null then ExceptionBuilder.RaiseMRException(ExceptionBuilder.EmptyActionProcessors)
                    
                    // create the context for this action processment
                    let ctx = ActionExecutionContext(action, controller, context, route_data)

                    // Run
                    (!firstMsg).Process ctx 

                finally
                    if _lifetime <> null then 
                        _lifetime.Dispose()
                        _lifetime <- null



