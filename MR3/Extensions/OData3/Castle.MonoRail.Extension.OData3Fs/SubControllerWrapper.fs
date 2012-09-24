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

namespace Castle.MonoRail

    open System
    open System.Reflection
    open System.Collections.Generic
    open System.Linq
    open Castle.MonoRail
    open Castle.MonoRail.OData
    open Castle.MonoRail.OData.Internal
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl


    type internal SubControllerWrapper(entType:Type, creator:Func<ControllerCreationContext, ControllerPrototype>) = 

        let _serviceOps : Ref<MethodInfoActionDescriptor []> = ref null
        let _typesMentioned = HashSet<Type>()

        let buildFunctionImport (model:IEdmModel) (action:MethodInfoActionDescriptor) type2EdmType type2EdmSet : IEdmFunctionImport = 
            EdmModelBuilder.build_function_import model action type2EdmType type2EdmSet

        let should_collect_type (t) = 
            if t <> typeof<unit> && t <> typeof<System.Void> && not <| t.IsPrimitive && t <> typeof<string> 
            then true else false

        let inspect_action (d:MethodInfoActionDescriptor) = 
            if should_collect_type d.ReturnType then
                _typesMentioned.Add (d.ReturnType) |> ignore
            d.Parameters 
            |> Seq.iter (fun p -> if should_collect_type p.ParamType then _typesMentioned.Add (p.ParamType) |> ignore) 

        let collect_mentioned_types () =
            !_serviceOps |> Array.iter inspect_action

        do
            if creator <> null then
                let dummyCtx = new ControllerCreationContext(null, null)
                let prototype = creator.Invoke(dummyCtx) :?> TypedControllerPrototype
                let desc = prototype.Descriptor
                _serviceOps := desc.Actions 
                               |> Seq.filter (fun action -> action.HasAnnotation<ODataOperationAttribute>()) 
                               |> Seq.cast<MethodInfoActionDescriptor>
                               |> Seq.toArray
                collect_mentioned_types ()
            ()

        member x.TargetType = entType

        member x.TypesMentioned : seq<Type> = upcast _typesMentioned

        member x.BuildFunctionImports (edmModel:IEdmModel, type2EdmType, type2EdmSet)  : seq<IEdmFunctionImport> = 
            if !_serviceOps = null then
                Seq.empty
            else
                let container = edmModel.EntityContainers().ElementAt(0)
                !_serviceOps 
                |> Array.map (fun a -> buildFunctionImport edmModel a type2EdmType type2EdmSet ) 
                |> Array.toSeq
            
        member x.Invoke(contextCreator:Func<ControllerCreationContext>, 
                        action:string, isColl:bool, parameters:(Type*obj) seq, 
                        value:obj, isOptional:bool) = 
            
            // let executor = (!_services).ControllerExecutorProvider.CreateExecutor(prototype)
            (* 
            let odataExecutor = executor :?> ODataEntitySubControllerExecutor
                (fun action isCollection parameters routeMatch context -> 
                    let callback = Func<Type,obj>(fun ptype -> tryResolveParamValue ptype isCollection parameters)
                    odataExecutor.GetParameterCallback <- callback
                    executor.Execute(action, prototype, routeMatch, context))
            *)

            // let executor = create_executor_fn rt prototype 
            // _invoker_cache.[rt] <- executor
            
            true, null

