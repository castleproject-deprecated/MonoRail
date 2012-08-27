
module EdmTypeSystem

    open System
    open System.IO
    open System.Reflection
    open System.Collections.Generic
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library


    let edmTypeComparer = 
        { new IEqualityComparer<IEdmType> with
              member x.Equals(a, b) = 
                  if a.TypeKind = b.TypeKind then
                    match a.TypeKind with
                    | EdmTypeKind.Entity ->
                        let left  = a :?> IEdmEntityType
                        let right = b :?> IEdmEntityType 
                        right.FullName() = left.FullName() 
                    
                    | EdmTypeKind.EntityReference ->
                        let left  = a :?> IEdmEntityReferenceType
                        let right = b :?> IEdmEntityReferenceType
                        x.Equals(left.EntityType, right.EntityType)
                    
                    | EdmTypeKind.Collection ->
                        let left  = a :?> IEdmCollectionType
                        let right = b :?> IEdmCollectionType
                        x.Equals(left.ElementType.Definition, right.ElementType.Definition)
                    
                    | EdmTypeKind.Complex ->
                        let left  = a :?> IEdmComplexType
                        let right = b :?> IEdmComplexType
                        right.FullName() = left.FullName()
                    
                    | EdmTypeKind.Enum ->
                        let left  = a :?> IEdmEnumType
                        let right = b :?> IEdmEnumType
                        right.FullName() = left.FullName() 
                    
                    | EdmTypeKind.Primitive ->
                        let left  = a :?> IEdmPrimitiveType
                        let right = b :?> IEdmPrimitiveType
                        right.FullName() = left.FullName() 

                    | _ -> failwithf "TypeKind not supported %O" a.TypeKind
                  else 
                    false
              member x.GetHashCode(a) = 
                  a.GetHashCode()
        }

    let edmTypeRefComparer = 
        { new IEqualityComparer<IEdmTypeReference> with
              member x.Equals(a, b) = 
                  edmTypeComparer.Equals(a.Definition, b.Definition)
              member x.GetHashCode(a) = 
                  a.GetHashCode()
        }


    let private PrimitiveTypeReferenceMap = Dictionary<Type, IEdmPrimitiveTypeReference>(EqualityComparer<Type>.Default)

    let private ToTypeReference(primitiveType:IEdmPrimitiveType, nullable:bool) : EdmPrimitiveTypeReference = 
      match primitiveType.PrimitiveKind with
      | EdmPrimitiveTypeKind.Binary -> 
            upcast EdmBinaryTypeReference(primitiveType, nullable)
      | EdmPrimitiveTypeKind.Boolean 
      | EdmPrimitiveTypeKind.Byte
      | EdmPrimitiveTypeKind.Double
      | EdmPrimitiveTypeKind.Guid
      | EdmPrimitiveTypeKind.Int16
      | EdmPrimitiveTypeKind.Int32
      | EdmPrimitiveTypeKind.Int64
      | EdmPrimitiveTypeKind.SByte
      | EdmPrimitiveTypeKind.Single
      | EdmPrimitiveTypeKind.Stream -> 
            EdmPrimitiveTypeReference(primitiveType, nullable)
      | EdmPrimitiveTypeKind.DateTime
      | EdmPrimitiveTypeKind.DateTimeOffset
      | EdmPrimitiveTypeKind.Time ->
            upcast EdmTemporalTypeReference(primitiveType, nullable)
      | EdmPrimitiveTypeKind.Decimal ->
            upcast EdmDecimalTypeReference(primitiveType, nullable)
      | EdmPrimitiveTypeKind.String ->
            upcast EdmStringTypeReference(primitiveType, nullable)
      | EdmPrimitiveTypeKind.Geography
      | EdmPrimitiveTypeKind.GeographyPoint
      | EdmPrimitiveTypeKind.GeographyLineString
      | EdmPrimitiveTypeKind.GeographyPolygon
      | EdmPrimitiveTypeKind.GeographyCollection
      | EdmPrimitiveTypeKind.GeographyMultiPolygon
      | EdmPrimitiveTypeKind.GeographyMultiLineString
      | EdmPrimitiveTypeKind.GeographyMultiPoint
      | EdmPrimitiveTypeKind.Geometry
      | EdmPrimitiveTypeKind.GeometryPoint
      | EdmPrimitiveTypeKind.GeometryLineString
      | EdmPrimitiveTypeKind.GeometryPolygon
      | EdmPrimitiveTypeKind.GeometryCollection
      | EdmPrimitiveTypeKind.GeometryMultiPolygon
      | EdmPrimitiveTypeKind.GeometryMultiLineString
      | EdmPrimitiveTypeKind.GeometryMultiPoint -> 
            upcast EdmSpatialTypeReference(primitiveType, nullable);
      | _ -> null
        //raise(Exception("System.Data.Services.Strings.General_InternalError((object) InternalErrorCodesCommon.EdmLibraryExtensions_PrimitiveTypeReference)"))

    PrimitiveTypeReferenceMap.Add(typeof<bool>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<bool>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean), true))
    PrimitiveTypeReferenceMap.Add(typeof<byte>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Byte), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<byte>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Byte), true))
    PrimitiveTypeReferenceMap.Add(typeof<DateTime>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTime), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<DateTime>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTime), true))
    PrimitiveTypeReferenceMap.Add(typeof<DateTimeOffset>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<DateTimeOffset>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), true))
    PrimitiveTypeReferenceMap.Add(typeof<Decimal>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Decimal), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<Decimal>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Decimal), true))
    PrimitiveTypeReferenceMap.Add(typeof<double>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<double>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double), true))
    PrimitiveTypeReferenceMap.Add(typeof<int16>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int16), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<int16>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int16), true))
    PrimitiveTypeReferenceMap.Add(typeof<int>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<int>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), true))
    PrimitiveTypeReferenceMap.Add(typeof<int64>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<int64>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64), true))
    PrimitiveTypeReferenceMap.Add(typeof<sbyte>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.SByte), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<sbyte>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.SByte), true))
    PrimitiveTypeReferenceMap.Add(typeof<string>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String), true))
    PrimitiveTypeReferenceMap.Add(typeof<single>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<single>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single), true))
    PrimitiveTypeReferenceMap.Add(typeof<byte[]>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Binary), true))
    PrimitiveTypeReferenceMap.Add(typeof<Stream>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Stream), false))
    PrimitiveTypeReferenceMap.Add(typeof<Guid>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<Guid>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid), true))
    PrimitiveTypeReferenceMap.Add(typeof<TimeSpan>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Time), false))
    PrimitiveTypeReferenceMap.Add(typeof<Nullable<TimeSpan>>, ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Time), true))


    let GetPrimitiveTypeReference(clrType:Type) : IEdmPrimitiveTypeReference =
        let succ,res = PrimitiveTypeReferenceMap.TryGetValue(clrType)
        if succ then res
        else null
        (*
          IEdmPrimitiveType primitiveType = (IEdmPrimitiveType) null;
          if (typeof<GeographyPoint).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint);
          else if (typeof<GeographyLineString).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString);
          else if (typeof<GeographyPolygon).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon);
          else if (typeof<GeographyMultiPoint).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint);
          else if (typeof<GeographyMultiLineString).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString);
          else if (typeof<GeographyMultiPolygon).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon);
          else if (typeof<GeographyCollection).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);
          else if (typeof<Geography).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);
          else if (typeof<GeometryPoint).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint);
          else if (typeof<GeometryLineString).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString);
          else if (typeof<GeometryPolygon).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon);
          else if (typeof<GeometryMultiPoint).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint);
          else if (typeof<GeometryMultiLineString).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString);
          else if (typeof<GeometryMultiPolygon).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon);
          else if (typeof<GeometryCollection).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);
          else if (typeof<Geometry).IsAssignableFrom(clrType))
            primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);
          if (primitiveType == null)
            return (IEdmPrimitiveTypeReference) null;
          else
            return ToTypeReference(primitiveType, true);
            *)
    

