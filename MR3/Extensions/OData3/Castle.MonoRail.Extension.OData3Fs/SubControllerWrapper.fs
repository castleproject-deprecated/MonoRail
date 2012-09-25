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


    type internal SubControllerWrapper(entType:Type, 
                                       creator:Func<ControllerCreationContext, ControllerPrototype>,
                                       executorProvider:ControllerExecutorProviderAggregator) = 

        let _serviceOps : Ref<MethodInfoActionDescriptor []> = ref null
        let _typesMentioned = HashSet<Type>()
        let mutable _desc : TypedControllerDescriptor = null

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

        // we will have issues with object models with self referencies
        // a better implementation would "consume" the items used, taking them off the list
        let tryResolveParamValue (paramType:Type) isCollection (parameters:(Type * obj) seq) = 
            let entryType =
                if isCollection then
                    match InternalUtils.getEnumerableElementType paramType with
                    | Some t -> t
                    | _ -> paramType
                elif paramType.IsGenericType then
                    paramType.GetGenericArguments().[0]
                else paramType

            match parameters |> Seq.tryFind (fun (ptype, _) -> ptype = entryType || entryType.IsAssignableFrom(ptype)) with 
            | Some (_, value) ->
                // param is Model<T>
                if paramType.IsGenericType && paramType.GetGenericTypeDefinition() = typedefof<Model<_>> 
                then Activator.CreateInstance ((typedefof<Model<_>>).MakeGenericType(paramType.GetGenericArguments()), [|value|])
                else // entryType <> paramType && paramType.IsAssignableFrom(entryType) then
                    value
            | _ -> null


        do
            if creator <> null then
                let dummyCtx = new ControllerCreationContext(null, null)
                let prototype = creator.Invoke(dummyCtx) :?> TypedControllerPrototype
                _desc <- prototype.Descriptor :?> TypedControllerDescriptor
                _serviceOps := _desc.Actions 
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
                        action:string, isCollection:bool, parameters:(Type*obj) seq, 
                        value:obj, isOptional:bool) = 
            
            if _desc.HasAction(action) then
                let ctx = contextCreator.Invoke()
                let prototype = creator.Invoke (ctx)

                // executor needs to be disposed
                use executor = executorProvider.CreateExecutor(prototype)

                let odataExecutor = executor :?> ODataEntitySubControllerExecutor

                let callback = Func<Type,obj>(fun ptype -> tryResolveParamValue ptype isCollection parameters)
                odataExecutor.GetParameterCallback <- callback
                let result = odataExecutor.Execute(action, prototype, ctx.RouteMatch, ctx.HttpContext)
            
                true, result
            else
                if isOptional 
                then true, null
                else false, null

