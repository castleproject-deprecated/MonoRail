module EdmModelBuilder

    open System
    open System.Reflection
    open System.Collections.Generic
    // open System.Data.OData
    // open System.Data.Services.Providers
    open System.Linq
    open Castle.MonoRail.OData
    open Castle.MonoRail.OData.Internal
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl

    let private PropertiesBindingFlags = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy

    // functionResolver:Func<Type, EdmFunctionImport seq>
    let build (schemaNamespace, containerName, entities:EntitySetConfig seq, extraTypes:Type seq, functionResolver:Func<Type, IEdmModel, IEdmFunctionImport seq>) = 
        
        let coreModel = EdmCoreModel.Instance
        let edmModel = EdmModel()
        edmModel.SetDataServiceVersion(Version(3,0))

        let edmContainer = EdmEntityContainer(schemaNamespace, containerName)
        // edmModel.AddReferencedModel(coreModel)
        edmModel.AddElement edmContainer

        let build_edmtype (name) (targetType:Type) : IEdmType = 
            let hasKeyProp = 
                targetType.GetProperties(PropertiesBindingFlags) 
                |> Seq.exists (fun p -> p.IsDefined(typeof<System.ComponentModel.DataAnnotations.KeyAttribute>, true) )
            if hasKeyProp then
                upcast TypedEdmEntityType(schemaNamespace, name, targetType)
            else
                upcast TypedEdmComplexType(schemaNamespace, name, targetType)

        let entityTypes = 
            entities
            |> Seq.map (fun e -> e.TargetType)
            |> Seq.append extraTypes
            |> Seq.toArray
            
        let edmTypeDefinitionsWithSets = 
            entities 
            |> Seq.map (fun ent -> ent, build_edmtype(ent.EntityName) (ent.TargetType) :?> TypedEdmEntityType )
            |> Seq.toArray

        let edmTypeDefinitionsForExtraTypes = 
            extraTypes 
            |> Seq.map (fun t -> build_edmtype(t.Name) t :?> TypedEdmEntityType )
            |> Seq.toArray

        let allEdmTypes = 
            edmTypeDefinitionsWithSets 
            |> Seq.map (fun (cfg,edm) -> edm)
            |> Seq.append edmTypeDefinitionsForExtraTypes
            |> Seq.toArray

        let edmTypeDefMap = 
            allEdmTypes.ToDictionary((fun (t:TypedEdmEntityType) -> t.TargetType), (fun t -> t |> box :?> IEdmType))
            

        let get_element_type (entTypeName:string) = 
            allEdmTypes 
            |> Seq.find (fun def -> def.Name = entTypeName)

        let edmSetDefinitions = 
            entities 
            |> Seq.map (fun ent -> ent, edmContainer.AddEntitySet(ent.EntitySetName, get_element_type(ent.TargetType.Name)) |> ignore)
            |> Array.ofSeq



        let rec process_properties_and_navigations (entDef:EdmStructuredType) (processed:HashSet<_>) = 
            // entSetConfig.PropertiesToIgnore
            // entSetConfig.CustomPropConfig
            // entSetConfig.EntityPropertyAttributes

            let targetType = (entDef |> box :?> IEdmReflectionTypeAccessor).TargetType

            if not <| processed.Contains(entDef) then 

                processed.Add entDef |> ignore

                let keyProperties = List<IEdmStructuralProperty>()
                let properties = targetType.GetProperties(PropertiesBindingFlags)

                for prop in properties do
                    let isCollection, elType = 
                        match InternalUtils.getEnumerableElementType (prop.PropertyType) with 
                        | Some elType -> true, elType
                        | _ -> false, prop.PropertyType

                    let primitiveTypeRef = EdmTypeSystem.GetPrimitiveTypeReference(elType)
                    if primitiveTypeRef <> null then
                        if isCollection then
                            ()
                        else
                            let primitiveProp = entDef.AddStructuralProperty(prop.Name, primitiveTypeRef) 
                            if prop.IsDefined(typeof<System.ComponentModel.DataAnnotations.KeyAttribute>, true) then
                                keyProperties.Add(primitiveProp)
                    else
                        let succ, _ = edmTypeDefMap.TryGetValue(elType)
                        if not succ then
                            // needs to build type
                            let edmType = build_edmtype (elType.Name) (elType)
                            edmTypeDefMap.[elType] <- edmType
                            process_properties_and_navigations (edmType :?> EdmStructuredType) processed

                        let _, otherTypeDef = edmTypeDefMap.TryGetValue(elType)

                        let otherTypeDef = otherTypeDef :?> EdmStructuredType

                        ()

                        (*
                        if otherTypeDef = entDef then
                            // self relation
                            let pi = EdmNavigationPropertyInfo()
                            pi.Name <- prop.Name
                            pi.Target <- entDef |> box :?> IEdmEntityType
                            pi.TargetMultiplicity <- if isCollection then EdmMultiplicity.Many else EdmMultiplicity.ZeroOrOne

                            let otherside = EdmNavigationPropertyInfo()
                            otherside.Name <- (entDef |> box :?> EdmEntityType).Name
                            otherside.TargetMultiplicity <- if isCollection then EdmMultiplicity.ZeroOrOne else EdmMultiplicity.Many

                            (entDef |> box :?> EdmEntityType).AddUnidirectionalNavigation(pi, otherside) |> ignore

                        else
                            // ensure otherside was processed as well
                            process_properties_and_navigations otherTypeDef processed

                            // otherside side
                            let other = EdmNavigationPropertyInfo()
                            other.Name <- prop.Name
                            other.Target <- otherTypeDef |> box :?> IEdmEntityType
                            other.TargetMultiplicity <- if isCollection then EdmMultiplicity.Many else EdmMultiplicity.ZeroOrOne

                            // Looks like MS considers everything many to many
                            // even if there's a counterpart relation in the other end, so we will mimic that

                            // this side
                            let thisside = EdmNavigationPropertyInfo()
                            thisside.Target <- entDef |> box :?> IEdmEntityType
                            thisside.Name <- (entDef |> box :?> EdmEntityType).Name // ideally as plural!
                            thisside.TargetMultiplicity <- EdmMultiplicity.Many
                                
                            (entDef |> box :?> EdmEntityType).AddUnidirectionalNavigation(other, thisside) |> ignore
                        *)

                if keyProperties.Count > 0 then    
                    (entDef |> box :?> EdmEntityType).AddKeys(keyProperties)

        let processed = HashSet<_>()

        allEdmTypes |> Seq.iter (fun entDef -> process_properties_and_navigations entDef processed )
        allEdmTypes |> Seq.iter (fun entDef -> edmModel.AddElement(entDef))

        let edmFunctions = 
            edmTypeDefinitionsWithSets 
            |> Seq.collect (fun (_,entDef) -> functionResolver.Invoke(entDef.TargetType, edmModel))
        edmFunctions |> Seq.iter (fun funImport -> edmContainer.AddElement(funImport) )

        edmModel


