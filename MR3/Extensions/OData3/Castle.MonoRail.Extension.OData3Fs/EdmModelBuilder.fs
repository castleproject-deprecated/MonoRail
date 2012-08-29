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

namespace Castle.MonoRail.OData.Internal

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


    module EdmModelBuilder = 

        let private PropertiesBindingFlags = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy

        let private build_edmtype (container:EdmModel) (schemaNamespace) (name) (targetType:Type) : IEdmType = 
            let hasKeyProp = 
                targetType.GetProperties(PropertiesBindingFlags) 
                |> Seq.exists (fun p -> p.IsDefined(typeof<System.ComponentModel.DataAnnotations.KeyAttribute>, true) )
            let edmType : IEdmType = 
                if hasKeyProp then
                    upcast TypedEdmEntityType(schemaNamespace, name, targetType)
                else
                    upcast TypedEdmComplexType(schemaNamespace, name, targetType)
            container.AddElement (edmType :?> IEdmSchemaElement)
            edmType


        let rec private process_properties_and_navigations (config:EntitySetConfig option) (entDef:EdmStructuredType) 
                                                           (edmTypeDefMap:Dictionary<Type, IEdmType>) (processed:HashSet<_>) buildType = 
            // TODO:
            // entSetConfig.CustomPropConfig
            // entSetConfig.EntityPropertyAttributes

            let targetType = (entDef |> box :?> IEdmReflectionTypeAccessor).TargetType

            if not <| processed.Contains(entDef) then 

                processed.Add entDef |> ignore

                let propertiesToIgnore = 
                    match config with 
                    | Some c -> c.PropertiesToIgnore
                    | _ -> List<_>()

                let keyProperties = List<IEdmStructuralProperty>()
                let properties = targetType.GetProperties(PropertiesBindingFlags)

                for prop in properties do
                    if not <| propertiesToIgnore.Contains(prop) then
                        let isCollection, elType = 
                            match InternalUtils.getEnumerableElementType (prop.PropertyType) with 
                            | Some elType -> true, elType
                            | _ -> false, prop.PropertyType

                        let primitiveTypeRef = EdmTypeSystem.GetPrimitiveTypeReference(elType)

                        if primitiveTypeRef <> null then
                            if isCollection then
                                failwith "Support for collection of primitives is missing"
                            else
                                let primitiveProp = entDef.AddStructuralProperty(prop.Name, primitiveTypeRef) 
                                if prop.IsDefined(typeof<System.ComponentModel.DataAnnotations.KeyAttribute>, true) then
                                    keyProperties.Add(primitiveProp)
                        else
                            let succ, _ = edmTypeDefMap.TryGetValue(elType)
                            if not succ then
                                // needs to build type
                                let edmType = buildType (elType.Name) (elType)
                                edmTypeDefMap.[elType] <- edmType
                                process_properties_and_navigations None (edmType |> box :?> EdmStructuredType) edmTypeDefMap processed buildType

                            let _, otherTypeDef = edmTypeDefMap.TryGetValue(elType)

                            let otherTypeDef = otherTypeDef :?> EdmStructuredType

                            if otherTypeDef.IsComplex then
                            
                                let complexTypeDef = otherTypeDef |> box :?> IEdmComplexType
                                let refType = EdmComplexTypeReference(complexTypeDef, true)

                                if not isCollection then
                                    entDef.AddStructuralProperty(prop.Name, refType) |> ignore
                                else
                                    entDef.AddStructuralProperty(prop.Name, EdmCoreModel.GetCollection(refType)) |> ignore 

                            elif otherTypeDef.IsEntity then
                            
                                // let otherSideAsEntType = otherTypeDef
                                let thisAsEdmType = (entDef |> box :?> EdmEntityType)

                                if otherTypeDef = entDef then
                                    // self relation
                                    let pi = EdmNavigationPropertyInfo()
                                    pi.Name <- prop.Name
                                    pi.Target <- thisAsEdmType
                                    pi.TargetMultiplicity <- if isCollection then EdmMultiplicity.Many else EdmMultiplicity.ZeroOrOne

                                    let otherside = EdmNavigationPropertyInfo()
                                    otherside.Name <- thisAsEdmType.Name
                                    otherside.TargetMultiplicity <- if isCollection then EdmMultiplicity.ZeroOrOne else EdmMultiplicity.Many

                                    thisAsEdmType.AddUnidirectionalNavigation(pi, otherside) |> ignore

                                else
                                    // ensure otherside was processed as well
                                    // process_properties_and_navigations otherTypeDef processed

                                    let otherAsEdmType = (otherTypeDef |> box :?> EdmEntityType)

                                    // otherside side
                                    let other = EdmNavigationPropertyInfo()
                                    other.Name <- prop.Name
                                    other.Target <- otherAsEdmType
                                    other.TargetMultiplicity <- if isCollection then EdmMultiplicity.Many else EdmMultiplicity.ZeroOrOne

                                    // Looks like MS considers everything many to many
                                    // even if there's a counterpart relation in the other end, so we will mimic that

                                    // this side
                                    let thisside = EdmNavigationPropertyInfo()
                                    thisside.Target <- thisAsEdmType
                                    thisside.Name <- thisAsEdmType.Name // ideally as plural!
                                    thisside.TargetMultiplicity <- EdmMultiplicity.Many
                                
                                    thisAsEdmType.AddUnidirectionalNavigation(other, thisside) |> ignore
                        

                if keyProperties.Count > 0 then    
                    (entDef |> box :?> EdmEntityType).AddKeys(keyProperties)

        // functionResolver:Func<Type, EdmFunctionImport seq>
        let build (schemaNamespace, containerName, entities:EntitySetConfig seq, extraTypes:Type seq, functionResolver:Func<Type, IEdmModel, IEdmFunctionImport seq>) = 
        
            let coreModel = EdmCoreModel.Instance
            let edmModel = EdmModel()
            edmModel.SetDataServiceVersion(Version(3,0))

            let edmContainer = EdmEntityContainer(schemaNamespace, containerName)
            // edmModel.AddReferencedModel(coreModel)
            edmModel.AddElement edmContainer

            // I LOVE currying
            let build_type = build_edmtype edmModel schemaNamespace

            let entityTypes = 
                entities
                |> Seq.map (fun e -> e.TargetType)
                |> Seq.append extraTypes
                |> Seq.toArray
            
            let edmTypeDefinitionsWithSets = 
                entities 
                |> Seq.map (fun ent -> ent, build_type (ent.EntityName) (ent.TargetType) :?> TypedEdmEntityType )
                |> Seq.toArray

            let edmTypeDefinitionsForExtraTypes = 
                extraTypes 
                |> Seq.map (fun t -> build_type (t.Name) t :?> TypedEdmEntityType )
                |> Seq.toArray

            let allEdmTypes = 
                edmTypeDefinitionsWithSets 
                |> Seq.map (fun (cfg,edm) -> edm)
                |> Seq.append edmTypeDefinitionsForExtraTypes
                |> Seq.toArray

            let edmTypeDefMap = 
                allEdmTypes.ToDictionary((fun (t:TypedEdmEntityType) -> t.TargetType), (fun t -> t |> box :?> IEdmType))

            let type2Config = 
                entities.ToDictionary((fun (t:EntitySetConfig) -> t.TargetType), (fun t -> t))
            

            let get_element_type (entTypeName:string) = 
                allEdmTypes 
                |> Seq.find (fun def -> def.Name = entTypeName)

            let edmSetDefinitions = 
                entities 
                |> Seq.map (fun ent -> ent, edmContainer.AddEntitySet(ent.EntitySetName, get_element_type(ent.TargetType.Name)) |> ignore)
                |> Array.ofSeq

            let processed = HashSet<_>()

            allEdmTypes 
                |> Seq.iter (fun entDef -> 
                                let _, config = type2Config.TryGetValue(entDef.TargetType)
                                let configVal = 
                                    if config = null then None else Some(config)
                                process_properties_and_navigations configVal entDef edmTypeDefMap processed build_type
                            )

            let edmFunctions = 
                edmTypeDefinitionsWithSets 
                |> Seq.collect (fun (_,entDef) -> functionResolver.Invoke(entDef.TargetType, edmModel))
            edmFunctions |> Seq.iter (fun funImport -> edmContainer.AddElement(funImport) )

            edmModel

