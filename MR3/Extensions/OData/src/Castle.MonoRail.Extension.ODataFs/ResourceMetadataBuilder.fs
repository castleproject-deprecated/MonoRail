
namespace Castle.MonoRail.OData

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Data.Services.Common
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection

    [<AllowNullLiteral>]
    type EntitySetConfig(entityName, source, targetType:Type) = 
        let _entMapAttrs : List<EntityPropertyMappingAttribute> = List()
        member x.TargetType = targetType
        member x.EntityName : string = entityName
        member x.Source : IQueryable = source
        member internal x.EntityPropertyAttributes : List<EntityPropertyMappingAttribute> = _entMapAttrs


    and EntitySetConfigurator<'a>(entityName, source:IQueryable<'a>) = 
        inherit EntitySetConfig(entityName, source, typeof<'a>)
        
        member x.TypedSource = source

        member x.AddAttribute( att:EntityPropertyMappingAttribute ) = 
            //let memberAccess = exp.Body :?> MemberExpression
            //let prop = memberAccess.Member :?> PropertyInfo 
            //let res, list = x.EntityPropertyAttributes.TryGetValue prop
            //if res
            //then list.Add att
            //else x.EntityPropertyAttributes.[prop] <- List([att]) 
            x.EntityPropertyAttributes.Add att


namespace Castle.MonoRail.Extension.OData

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Data.Services.Common
    open System.Linq
    open Castle.MonoRail.OData
    open System.Reflection
    open System.ComponentModel.DataAnnotations


    module ResourceMetadataBuilder =

        let private PropertiesBindingFlags = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy

        let private is_primitive (pType:Type) = 
            (ResourceType.GetPrimitiveResourceType pType) <> null

        let private resolve_name (pType:Type) (type2CustomName:Dictionary<Type, string>) = 
            let res, name = type2CustomName.TryGetValue(pType)
            if res then name
            else pType.Name

        let private resolve_resourceTypeKind_based_on_properties (entType:Type) = 
            let hasKeyOrNonPrimitives = 
                entType.GetProperties(PropertiesBindingFlags)
                |> Seq.exists (fun p -> p.IsDefined(typeof<KeyAttribute>, true) || not <| is_primitive p.PropertyType)
            if hasKeyOrNonPrimitives 
            then ResourceTypeKind.EntityType
            else ResourceTypeKind.ComplexType

        let private extract_elementType (pType:Type) = 
            if pType.UnderlyingSystemType.IsGenericType && pType.UnderlyingSystemType.GetGenericTypeDefinition() = typedefof<IEnumerable<_>> then
                pType.GetGenericArguments().[0]
            else
                let interType = pType.GetInterface(typedefof<IEnumerable<_>>.Name, false)
                if pType.IsGenericType && interType <> null 
                then pType.GetGenericArguments().[0]
                else null


        let rec private resolveRT (pType) (knownTypes:Dictionary<Type, ResourceType>) (builderFn) = 
            // maybe it's a primitive
            let resourceType = ResourceType.GetPrimitiveResourceType(pType)
            if resourceType <> null then 
                Some(resourceType, false)
            
            // maybe we know this type
            elif knownTypes.ContainsKey(pType) then
                Some(knownTypes.[pType], false)
            
            // maybe this type is actually a collection, and therefore we should do this for the element type
            else
                let elementType = extract_elementType(pType)
                if elementType <> null then
                    let subResolved = resolveRT elementType knownTypes builderFn
                    if Option.isSome subResolved
                    then Some(fst subResolved.Value, true)
                    else None
                else
                    // we have exhausted the easy options.  
                    // let's go ahead and try to construct the resource type
                    let result : ResourceType = builderFn (pType)
                    if result <> null 
                    then Some(result, false)
                    else None


        let private resolve_propertKind (resource:ResourceType) (propertyInfo:PropertyInfo) (isCollection) = 

            if resource.ResourceTypeKind = ResourceTypeKind.Primitive then
                if propertyInfo.IsDefined(typeof<KeyAttribute>, true) 
                then ResourcePropertyKind.Primitive ||| ResourcePropertyKind.Key
                else ResourcePropertyKind.Primitive 
            
            elif resource.ResourceTypeKind = ResourceTypeKind.ComplexType then
                ResourcePropertyKind.ComplexType
            
            elif resource.ResourceTypeKind = ResourceTypeKind.EntityType then
                if isCollection  
                then ResourcePropertyKind.ResourceSetReference  
                else ResourcePropertyKind.ResourceReference
            else 
                failwithf "Unsupported resource type (kind) for property"
            

        let private build_property (resource:ResourceType) (prop:PropertyInfo) (knownTypes:Dictionary<Type, ResourceType>) builderFn  = 
            
            if not prop.DeclaringType.IsInterface && prop.CanRead && prop.GetIndexParameters().Length = 0 then
                let propType = prop.PropertyType
                match resolveRT propType knownTypes builderFn with 
                | Some (resolvedType, isColl) ->
                    let kind = resolve_propertKind resolvedType prop isColl
                    let resProp = ResourceProperty(prop.Name, kind, resolvedType)
                    resource.AddProperty resProp
                | _ -> ()


        let private build_properties (resource:ResourceType) (knownTypes:Dictionary<Type, ResourceType>) (type2CustomName:Dictionary<Type, string>) 
                                     resourceBuilderFn (entType:Type)  = 
            
            entType.GetProperties(PropertiesBindingFlags) 
            |> Seq.iter (fun prop -> build_property resource prop knownTypes resourceBuilderFn)   


        let rec private build_resource_type schemaNs (knownTypes:Dictionary<Type, ResourceType>) (type2CustomName:Dictionary<Type, string>) 
                                            (entMapAttributes:List<EntityPropertyMappingAttribute>) (entType:Type) = 
            
            if entType.IsValueType || not entType.IsVisible || entType.IsArray || entType.IsPointer || entType.IsCOMObject || entType.IsInterface || 
                 entType = typeof<IntPtr> || entType = typeof<UIntPtr> || entType = typeof<char> || entType = typeof<TimeSpan> || 
                 entType = typeof<DateTimeOffset> || entType = typeof<Uri> || entType.IsEnum then 
               null
            elif knownTypes.ContainsKey(entType) then 
                knownTypes.[entType]
            else 
                // note: no support for hierarchies of resource types yet
                
                let kind = resolve_resourceTypeKind_based_on_properties entType
                let entityName = resolve_name entType type2CustomName
                let resource = ResourceType(entType, kind, null, schemaNs, entityName, false)
                knownTypes.[entType] <- resource
                entMapAttributes |> Seq.iter (fun e -> resource.AddEntityPropertyMappingAttribute e)

                build_properties resource knownTypes type2CustomName (build_resource_type schemaNs knownTypes type2CustomName entMapAttributes) entType
                resource


        /// Asserts that the return from build_resource_type is non null and EntityType
        let private build_entity_resource schemaNs (config:EntitySetConfig) (knownTypes) (type2CustomName) = 
            let resource = build_resource_type schemaNs knownTypes type2CustomName config.EntityPropertyAttributes config.TargetType

            if resource = null || resource.ResourceTypeKind <> ResourceTypeKind.EntityType 
            then failwithf "Expecting an entity to be constructed from %O but instead got something else" config.TargetType


        let build(schemaNs:string, configs:EntitySetConfig seq) = 

            let type2CustomName = 
                Enumerable.ToDictionary(configs, 
                                        (fun (c:EntitySetConfig) -> c.TargetType), 
                                        (fun (c:EntitySetConfig) -> c.EntityName))
            let knownTypes = Dictionary<Type, ResourceType>()

            configs |> Seq.iter (fun c -> build_entity_resource schemaNs c knownTypes type2CustomName) 
            
            knownTypes.Values |> box :?> ResourceType seq
            



    