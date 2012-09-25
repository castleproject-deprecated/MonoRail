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
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl



    [<Interface>]
    type IEdmReflectionTypeAccessor = 
        abstract member TargetType : System.Type with get

    type TypedEdmEntityType(namespace_, name, typ:Type) = 
        inherit EdmEntityType(namespace_, name)

        member x.TargetType = typ

        interface IEdmReflectionTypeAccessor with
            member x.TargetType = typ

    type TypedEdmEnumType(namespace_, name, underlyingType:IEdmPrimitiveType, typ:Type) = 
        inherit EdmEnumType(namespace_, name, underlyingType, typ.IsDefined(typeof<FlagsAttribute>, false))

        member x.TargetType = typ

        interface IEdmReflectionTypeAccessor with
            member x.TargetType = typ


    type TypedEdmComplexType(namespace_, name, typ:Type) = 
        inherit EdmComplexType(namespace_, name)

        member x.TargetType = typ

        interface IEdmReflectionTypeAccessor with
            member x.TargetType = typ



    type TypedEdmStructuralProperty(declaringType, name, typeRef, propInfo:PropertyInfo, getExp, setExp) = 
        inherit EdmStructuralProperty(declaringType, name, typeRef)
        // TODO: add get/set for instance values

        let mutable _getExp : obj -> obj = getExp
        let mutable _setExp : obj -> obj -> unit = setExp

        member x.CanReflect = propInfo <> null
        member x.PropertyInfo = propInfo

        member x.GetValue(instance) = 
            _getExp(instance)
        member x.SetValue(instance, newValue) = 
            _setExp instance newValue
        

    type TypedEdmNavigationProperty(declaringType, name, typeRef, dependentProps, containsTarget, ondelete, propInfo:PropertyInfo, getExp, setExp) = 
        inherit EdmProperty(declaringType, name, typeRef)

        let mutable _partner : IEdmNavigationProperty = null
        let mutable _getExp : obj -> obj = getExp
        let mutable _setExp : obj -> obj -> unit = setExp

        override x.PropertyKind = EdmPropertyKind.Navigation

        member x.CanReflect = propInfo <> null
        member x.PropertyInfo = propInfo

        member x.GetValue(instance) = 
            _getExp(instance)
        member x.SetValue(instance, newValue) = 
            _setExp instance newValue

        member x.Partner with get() = _partner and set(v) = _partner <- v

        interface IEdmNavigationProperty with 
            member x.Partner = _partner
            member x.OnDelete = ondelete
            member x.DependentProperties = dependentProps
            member x.ContainsTarget = containsTarget
            member x.IsPrincipal = dependentProps == null && _partner <> null && _partner.DependentProperties <> null


    [<AutoOpen>]
    module EdmExtensions =

        // type augmentations 

        type Microsoft.Data.Edm.IEdmProperty with
            member x.CanReflect = 
                match x with 
                | :? TypedEdmStructuralProperty as p -> p.CanReflect
                | :? TypedEdmNavigationProperty as p -> p.CanReflect
                | _ -> failwithf "type %O is not a supported property type" (x)
            member x.PropertyInfo = 
                match x with 
                | :? TypedEdmStructuralProperty as p -> p.PropertyInfo
                | :? TypedEdmNavigationProperty as p -> p.PropertyInfo
                | _ -> failwithf "type %O is not a supported property type" (x)


        type Microsoft.Data.Edm.IEdmType with
            member x.IsComplex = x.TypeKind = EdmTypeKind.Complex
        
            member x.IsEntity = x.TypeKind = EdmTypeKind.Entity

            member x.FName = 
                match x with 
                | :? EdmEntityType as edm -> edm.FullName()
                | :? EdmComplexType as edm -> edm.FullName()
                | :? IEdmPrimitiveType as primitive -> primitive.Name
                | _ -> failwithf "type %s is not a subclass of EdmStructuredType" (x.GetType().FullName)

            member x.TargetType = 
                match x with 
                | :? IEdmReflectionTypeAccessor as accessor -> 
                    accessor.TargetType
                
                | :? IEdmPrimitiveType as primitive ->
                    EdmTypeSystem.GetClrTypeFromPrimitiveType (primitive)

                | :? IEdmCollectionType as coll ->
                    let elType = coll.ElementType.Definition.TargetType
                    typedefof<IQueryable<_>>.MakeGenericType([|elType|])

                | _ -> failwithf "type %s does not implement IEdmReflectionTypeAccessor" (x.GetType().FullName)


