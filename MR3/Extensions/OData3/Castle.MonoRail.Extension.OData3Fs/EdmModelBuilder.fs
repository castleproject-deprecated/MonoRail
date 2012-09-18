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
    open Microsoft.Data.Edm.Library.Values
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
                elif targetType.IsEnum then
                    let underlyingType = EdmTypeSystem.GetPrimitiveTypeReference (Enum.GetUnderlyingType(targetType))
                    let enumType = TypedEdmEnumType(schemaNamespace, name, underlyingType.Definition :?> IEdmPrimitiveType, targetType)
                    upcast enumType
                else
                    upcast TypedEdmComplexType(schemaNamespace, name, targetType)

            container.AddElement (edmType :?> IEdmSchemaElement)
            edmType

        let private createNavProperty (pi:EdmNavigationPropertyInfo) (partnerpi:EdmNavigationPropertyInfo) propInfo getFunc setFunc = 
            let createPropType (targetType) (multiplicity) (multiplicityParameterName) : IEdmTypeReference = 
                match multiplicity with
                | EdmMultiplicity.ZeroOrOne -> upcast EdmEntityTypeReference(targetType, true)
                | EdmMultiplicity.One       -> upcast EdmEntityTypeReference(targetType, false)
                | EdmMultiplicity.Many      -> upcast EdmCoreModel.GetCollection(EdmEntityTypeReference(targetType, false))
                | _ -> failwith "Unexpected EdmMultiplicity value"
                
            let end1 = 
                TypedEdmNavigationProperty(partnerpi.Target,
                    pi.Name,
                    createPropType pi.Target pi.TargetMultiplicity "propertyInfo.TargetMultiplicity",
                    pi.DependentProperties, pi.ContainsTarget, pi.OnDelete, 
                    propInfo, getFunc, setFunc)

            let end2 = 
                TypedEdmNavigationProperty(
                    pi.Target,
                    partnerpi.Name,
                    createPropType partnerpi.Target partnerpi.TargetMultiplicity "partnerInfo.TargetMultiplicity",
                    partnerpi.DependentProperties, partnerpi.ContainsTarget, partnerpi.OnDelete, 
                    propInfo, getFunc, setFunc)

            end1.Partner <- end2
            end2.Partner <- end1
            end1


        let rec private process_properties_and_navigations (config:EntitySetConfig option) 
                                                           (propConfig:Dictionary<PropertyInfo, PropConfigurator>)
                                                           (entDef:EdmStructuredType) 
                                                           (edmTypeDefMap:Dictionary<Type, IEdmType>) 
                                                           (type2EntSet:Dictionary<Type, EdmEntitySet>)
                                                           (processed:HashSet<_>) buildType = 
            
            // TODO: entSetConfig.EntityPropertyAttributes for atom mapping

            let targetType = (entDef |> box :?> IEdmReflectionTypeAccessor).TargetType

            if not <| processed.Contains(entDef) then 

                processed.Add entDef |> ignore

                let propertiesToIgnore = 
                    match config with 
                    | Some c -> c.PropertiesToIgnore
                    | _ -> HashSet()

                let keyProperties = List<IEdmStructuralProperty>()
                let properties = 
                    targetType.GetProperties(PropertiesBindingFlags) 
                    |> Seq.filter (fun p -> not <| propertiesToIgnore.Contains(p))


                for prop in properties do
                    let propType, mapping = 
                        if propConfig <> null then
                            let succ, mapping = propConfig.TryGetValue(prop)
                            if succ 
                            then mapping.MappedType, mapping
                            else prop.PropertyType, null
                        else prop.PropertyType, null

                    let isCollection, elType = 
                        match InternalUtils.getEnumerableElementType (propType) with 
                        | Some elType -> true, elType
                        | _ -> false, propType

                    let propInfo = 
                        if mapping = null then prop else null
                    let standardGet = 
                        if mapping = null 
                        then fun instance -> prop.GetValue(instance, null)
                        else mapping.GetValue
                    let standardSet = 
                        if mapping = null 
                        then fun instance value -> prop.SetValue(instance, value, null)
                        else mapping.SetValue

                    let primitiveTypeRef = EdmTypeSystem.GetPrimitiveTypeReference (elType)

                    if primitiveTypeRef <> null then
                        //
                        // primitive properties support
                        //

                        if isCollection then
                            
                            let collType = EdmCollectionTypeReference(EdmCollectionType(primitiveTypeRef), true)
                            let collProp = TypedEdmStructuralProperty(entDef, prop.Name, collType, propInfo, standardGet, standardSet)
                            entDef.AddProperty(collProp)

                        else
                            // let primitiveProp = entDef.AddStructuralProperty(prop.Name, primitiveTypeRef) 
                            let structuralProp = TypedEdmStructuralProperty(entDef, prop.Name, primitiveTypeRef, propInfo, standardGet, standardSet)
                            entDef.AddProperty(structuralProp)

                            if prop.IsDefined(typeof<System.ComponentModel.DataAnnotations.KeyAttribute>, true) then
                                System.Diagnostics.Debug.Assert( keyProperties.Count = 0, "we dont support types with composite keys" )
                                keyProperties.Add(structuralProp)
                        
                    elif elType.IsEnum then
                        //
                        // enum support
                        //

                        let succ, _ = edmTypeDefMap.TryGetValue(elType)
                        if not succ then
                            // needs to build type
                            let edmType = buildType (elType.Name) (elType) |> box :?> EdmEnumType
                            let values = Enum.GetValues(elType)

                            Enum.GetNames(elType) 
                            |> Array.mapi (fun i name -> name, values.GetValue(i) )
                            |> Array.iter (fun (name,value) -> edmType.AddMember(name, EdmIntegerConstant(System.Convert.ToInt64(value))  ) |> ignore )

                            edmTypeDefMap.[elType] <- edmType
                            
                        // TODO: missing support for nullable<enum>
                        let enumType = edmTypeDefMap.[elType]
                        let structuralProp = 
                            TypedEdmStructuralProperty(entDef, prop.Name, 
                                                        EdmEnumTypeReference(enumType :?> IEdmEnumType, false), 
                                                        propInfo, standardGet, standardSet)
                            
                        entDef.AddProperty(structuralProp)
                            
                    else
                        //
                        // navigation properties support
                        //

                        let succ, _ = edmTypeDefMap.TryGetValue(elType)
                        if not succ then
                            // needs to build type
                            let edmType = buildType (elType.Name) (elType)
                            edmTypeDefMap.[elType] <- edmType
                            process_properties_and_navigations None propConfig (edmType |> box :?> EdmStructuredType) edmTypeDefMap type2EntSet processed buildType

                        let _, otherTypeDef = edmTypeDefMap.TryGetValue(elType)

                        let otherTypeDef = otherTypeDef :?> EdmStructuredType

                        if otherTypeDef.IsComplex then
                            
                            let complexTypeDef = otherTypeDef |> box :?> IEdmComplexType
                            let refType = EdmComplexTypeReference(complexTypeDef, true)

                            entDef.AddProperty <| 
                                if not isCollection 
                                then TypedEdmStructuralProperty(entDef, prop.Name, refType, propInfo, standardGet, standardSet)
                                else TypedEdmStructuralProperty(entDef, prop.Name, EdmCoreModel.GetCollection(refType), propInfo, standardGet, standardSet)

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
                                otherside.Target <- thisAsEdmType
                                otherside.TargetMultiplicity <- if isCollection then EdmMultiplicity.ZeroOrOne else EdmMultiplicity.Many

                                let navProp = createNavProperty pi otherside propInfo standardGet standardSet
                                thisAsEdmType.AddProperty navProp

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

                                let navProp = createNavProperty other thisside propInfo standardGet standardSet
                                thisAsEdmType.AddProperty navProp

                                // adds a navigation target if both sides are entitysets
                                // this turns into associationsets later
                                let exists, entSet = type2EntSet.TryGetValue(thisAsEdmType.TargetType)
                                if exists then
                                    let alsoExists, other = type2EntSet.TryGetValue(otherAsEdmType.TargetType)
                                    if alsoExists then
                                        entSet.AddNavigationTarget(navProp, other)


                if keyProperties.Count > 0 then    
                    (entDef |> box :?> EdmEntityType).AddKeys(keyProperties)

        let build (schemaNamespace, containerName, entities:EntitySetConfig seq, extraTypes:Type seq, functionResolver:Func<Type, IEdmModel, IEdmFunctionImport seq>) = 
        
            let coreModel = EdmCoreModel.Instance
            let edmModel = EdmModel()
            edmModel.SetDataServiceVersion(Version(3,0))

            let edmContainer = EdmEntityContainer(schemaNamespace, containerName)
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
                |> Seq.map (fun ent -> ent, edmContainer.AddEntitySet(ent.EntitySetName, get_element_type(ent.TargetType.Name)))
                |> Array.ofSeq

            let type2EntSet = 
                edmSetDefinitions.ToDictionary((fun (t:EntitySetConfig,_) -> t.TargetType), (fun (_,entSet) -> entSet))


            let processed = HashSet<_>()

            allEdmTypes 
            |> Seq.iter (fun entDef -> 
                            let _, config = type2Config.TryGetValue(entDef.TargetType)
                            let configVal, propConfig = 
                                if config = null then None, null else Some(config), config.CustomPropConfig
                            process_properties_and_navigations configVal propConfig entDef edmTypeDefMap type2EntSet processed build_type
                        )

            
            let edmFunctions = 
                edmTypeDefinitionsWithSets 
                |> Seq.collect (fun (_,entDef) -> functionResolver.Invoke(entDef.TargetType, edmModel))
            edmFunctions |> Seq.iter (fun funImport -> edmContainer.AddElement(funImport) )

            edmModel

