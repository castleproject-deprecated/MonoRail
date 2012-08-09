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
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq
    open Castle.MonoRail.OData
    open Castle.MonoRail.Extension.OData
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed


    /// Access point to entities to be exposed by a single odata endpoint
    [<AbstractClass>]
    type ODataModel(schemaNamespace, containerName) = 

        let mutable _schemaNs = schemaNamespace
        let mutable _containerName = containerName

        let _entities = List<EntitySetConfig>()
        let _type2SubControllerInfo : Ref<Dictionary<Type, SubControllerInfo>> = ref null
        let _resourcetypes : Ref<ResourceType seq> = ref null
        let _resourcesets  : Ref<ResourceSet seq> = ref null
        let _entSeq : EntitySetConfig seq = upcast _entities

        let _frozen = ref false

        let assert_not_frozen() = 
            if !_frozen then raise(InvalidOperationException("Model was already initialize and therefore cannot be changed"))

        member x.SchemaNamespace with get() = schemaNamespace
        member x.ContainerName   with get() = containerName

        abstract member Initialize : unit -> unit

        member internal x.Initialize( services:IServiceRegistry ) = 
            assert_not_frozen()

            // give model a chance to initialize/configure the entities
            x.Initialize()
            
            let entityTypes = _entities |> Seq.map (fun e -> e.TargetType)

            // for each subcontroller found, creates a "creator function" and the controller descriptor
            let resolve_subcontrollerinfo (entityType:Type) = 
                let template = typedefof<IODataEntitySubController<_>>
                let concrete = template.MakeGenericType([|entityType|])
                let spec = PredicateControllerCreationSpec(fun t -> concrete.IsAssignableFrom(t))
                let creator = services.ControllerProvider.CreateController(spec)
                if creator <> null then 
                    let prototype = creator.Invoke() :?> TypedControllerPrototype
                    { creator = creator; desc = prototype.Descriptor :?> TypedControllerDescriptor }
                else
                    SubControllerInfo.Empty

            let build_subcontrollers_map () = 
                let dict = Dictionary<Type, SubControllerInfo>()
                entityTypes
                |> Seq.map (fun rt -> rt, resolve_subcontrollerinfo rt)
                |> Seq.filter (fun t -> snd t <> SubControllerInfo.Empty)
                |> Seq.iter (fun t -> dict.Add (fst t, snd t))
                dict

            _type2SubControllerInfo := build_subcontrollers_map ()

            let collect_service_ops (desc:TypedControllerDescriptor) = 
                // todo: filtering of common hooks (create, intercept, authorize)
                desc.Actions 
                |> Seq.filter (fun action -> action.HasAnnotation<ODataOperationAttribute>())

            let collectedServiceOps = 
                !_type2SubControllerInfo 
                |> Seq.map (fun info -> info.Value.desc)
                |> Seq.collect ( collect_service_ops )

            let extraTypesOnServiceOpsSignatures = 
                let extract_complextypes_from_action (action:ControllerActionDescriptor) = 
                    // TODO: need to support the parameters as well
                    // action.Parameters |> Seq.map (fun p -> p.ParamType)
                    [| action.ReturnType |]

                collectedServiceOps 
                |> Seq.collect ( extract_complextypes_from_action )
                |> Seq.filter (fun t -> not ( t.IsPrimitive || t == typeof<string> || t == typeof<unit> ) )

            // extraTypesOnServiceOpsSignatures 
            // |> Seq.iter (fun t -> _entities.Add (EntitySetConfig(t)) )

            let rts = ResourceMetadataBuilder.build(_schemaNs, _entities, extraTypesOnServiceOpsSignatures)
            
            // set everything as readonly
            rts |> Seq.iter (fun rt -> rt.SetReadOnly() )
                
            _resourcetypes := rts
            _resourcesets  := 
                rts 
                |> Seq.filter (fun rt -> rt.ResourceTypeKind = ResourceTypeKind.EntityType && (_entities |> Seq.exists (fun e -> e.EntityName === rt.Name) ) )
                |> Seq.map (fun rt -> (let name = (_entSeq |> Seq.find(fun e -> e.TargetType = rt.InstanceType)).EntitySetName 
                                       let rs = ResourceSet(name, rt)
                                       rs.SetReadOnly()
                                       rs 
                                      )
                           )
                |> box :?> ResourceSet seq

            _frozen := true


        member x.EntitySet<'a>(entitySetName:string, source:IQueryable<'a>) = 
            assert_not_frozen()
            let entityType = typeof<'a>
            let cfg = EntitySetConfigurator(entitySetName, entityType.Name, source)
            _entities.Add cfg
            cfg

        member x.Entities = _entSeq

        member internal x.ResourceSets  = !_resourcesets
        member internal x.ResourceTypes = !_resourcetypes
        member internal x.GetResourceType(name) = 
            x.ResourceTypes 
            |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )

        member internal x.GetResourceSet(name) = 
            x.ResourceSets 
            |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )
        member internal x.GetQueryable(rs:ResourceSet) = 
            match _entities |> Seq.tryFind (fun e -> StringComparer.OrdinalIgnoreCase.Equals(e.EntitySetName, rs.Name)) with
            | Some e -> e.Source
            | _ -> null
        member internal x.GetRelatedResourceSet (rt:ResourceType) =
            x.ResourceSets 
            |> Seq.tryFind (fun rs -> rs.ResourceType = rt)

        // TODO: refactor to get rid of duplication on the following 3 methods
        member internal x.SupportsAction (rt:ResourceType, name:string) = 
            if !_type2SubControllerInfo <> null then
                let succ, info = (!_type2SubControllerInfo).TryGetValue rt.InstanceType
                succ && info.desc.HasAction name
            else false
        member internal x.GetControllerCreator (rt:ResourceType) = 
            if !_type2SubControllerInfo <> null then
                let succ, info = (!_type2SubControllerInfo).TryGetValue rt.InstanceType
                if succ then info.creator
                else null
            else null
        member internal x.GetNestedOperation (rt:ResourceType, name:string) : ControllerActionOperation = 
            if !_type2SubControllerInfo <> null then
                let succ, info = (!_type2SubControllerInfo).TryGetValue rt.InstanceType
                if succ && info.desc.HasAction name // TODO: should use HTTP VERB to narrow options (??)
                then ControllerActionOperation(rt, name)
                else null
            else null
        

        interface IDataServiceMetadataProvider with 
            member x.ContainerNamespace = x.SchemaNamespace
            member x.ContainerName = x.ContainerName
            member x.ResourceSets = x.ResourceSets
            member x.Types = x.ResourceTypes
            member x.ServiceOperations = 
                // we dont support ops yet
                Seq.empty
            member x.GetDerivedTypes(resType) = 
                // we dont support hierarchies yet
                Seq.empty
            member x.HasDerivedTypes(resType) = 
                // we dont support hierarchies yet
                false
            member x.TryResolveResourceSet(name, rtToReturn) = 
                match x.GetResourceSet(name) with 
                | Some rt -> rtToReturn <- rt; true
                | None -> false
            member x.TryResolveResourceType(name, rtToReturn) = 
                match x.GetResourceType(name) with 
                | Some rt -> rtToReturn <- rt; true
                | None -> false 
            member x.TryResolveServiceOperation(name, opToReturn) = 
                // we dont support ops yet
                false
            member x.GetResourceAssociationSet(resSet, resType, property) = 
                let targetResType = property.ResourceType
                match x.ResourceSets |> Seq.tryFind (fun rs -> targetResType.InstanceType.IsAssignableFrom(rs.ResourceType.InstanceType)) with
                | Some containerResSet -> 
                    ResourceAssociationSet(resType.Name + "_" + property.Name, 
                                           ResourceAssociationSetEnd(resSet, resType, property), 
                                           ResourceAssociationSetEnd(containerResSet, targetResType, null))
                | _ -> null

        
    
        


