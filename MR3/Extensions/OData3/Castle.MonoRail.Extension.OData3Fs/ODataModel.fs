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
    open Castle.MonoRail.OData
    open Castle.MonoRail.OData.Internal
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl


    /// Access point to entities to be exposed by a single odata endpoint
    [<AbstractClass>]
    type ODataModel(schemaNamespace, containerName) = 

        let _model : Ref<IEdmModel> = ref null
        let _subControllers : Ref<SubControllerWrapper seq> = ref null
        let _edmType2SubController = Dictionary<IEdmType, SubControllerWrapper>()
        let _type2SubController    = Dictionary<Type, SubControllerWrapper>()

        let _frozen = ref false

        let assert_not_frozen() = 
            if !_frozen then raise(InvalidOperationException("Model was already initialize and therefore cannot be changed"))

        let _entities = List<EntitySetConfig>()

        let build_cache_dictionaries (edmModel:IEdmModel) = 
            edmModel.SchemaElements
            |> Seq.filter (fun s -> s.SchemaElementKind = EdmSchemaElementKind.TypeDefinition)
            |> Seq.cast<IEdmEntityType>
            |> Seq.iter (fun dt ->  match  !_subControllers |> Seq.tryFind(fun sc -> sc.TargetType = dt.TargetType) with
                                    | Some sc -> 
                                        let ttype = dt.TargetType
                                        _edmType2SubController.[dt] <- sc
                                        _type2SubController.[ttype] <- sc
                                    | _ -> ())


        member x.SchemaNamespace = schemaNamespace
        member x.ContainerName   = containerName

        member internal x.IsInitialized = !_frozen

        abstract member Initialize : unit -> unit

        member internal x.EdmModel = !_model

        member internal x.InitializeModels (services:IServiceRegistry) = 
            let template = typedefof<IODataEntitySubController<_>>
            let create_spec (entityType:Type) : ControllerCreationSpec = 
                let concrete = template.MakeGenericType([|entityType|])
                upcast PredicateControllerCreationSpec(fun t -> concrete.IsAssignableFrom(t))

            assert_not_frozen()

            // give model a chance to initialize/configure the entities
            x.Initialize()

            let entTypes = 
                _entities 
                |> Seq.map(fun e -> e.TargetType)
            
            _subControllers := 
                entTypes 
                |> Seq.map(fun e -> SubControllerWrapper(e, services.ControllerProvider.CreateController( create_spec(e) ) ))

            let typesMentionedInSubControllers = 
                !_subControllers 
                |> Seq.collect (fun sub -> sub.TypesMentioned) 
                |> Seq.distinct
                |> Seq.filter (fun t -> not <| ( entTypes |> Seq.exists (fun e -> t = e) ) ) 

            let opDiscover (t:Type) (m:IEdmModel) : IEdmFunctionImport seq = 
                match !_subControllers |> Seq.tryFind (fun sc -> sc.TargetType = t) with
                | Some sc -> sc.GetFunctionImports (m)
                | _ -> Seq.empty
            
            let edmModel = EdmModelBuilder.build (schemaNamespace, containerName, _entities, 
                                                  typesMentionedInSubControllers, 
                                                  Func<Type, IEdmModel,_>(opDiscover))

            build_cache_dictionaries edmModel

            
            _model := upcast edmModel
            
            _frozen := true


        member x.EntitySet<'a>(entitySetName:string, source:IQueryable<'a>) = 
            assert_not_frozen()
            let entityType = typeof<'a>
            let cfg = EntitySetConfigurator(entitySetName, entityType.Name, source)
            _entities.Add cfg
            cfg

        member internal x.InvokeSubController(action:string, isColl:bool, rt:IEdmType, parameters:(Type*obj) seq, value:obj, isOptional:bool) =
            let succ, sc = _edmType2SubController.TryGetValue(rt)
            if succ then
                let contextCreation = Func<ControllerCreationContext>(fun c -> null)
                sc.Invoke(contextCreation, action, isColl, parameters, value, isOptional)
            else
                true, null

        member internal x.EntitiesConfigs = _entities

        member internal x.GetQueryable(rs:IEdmEntitySet) = 
            match _entities |> Seq.tryFind (fun e -> StringComparer.OrdinalIgnoreCase.Equals(e.EntitySetName, rs.Name)) with
            | Some e -> e.Source
            | _ -> null

