//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Hosting.Mvc.Typed

    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Serialization
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type RoutingValueProvider [<ImportingConstructor>] (route_match:RouteMatch) = 

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 
                let res, routeVal = route_match.RouteParams.TryGetValue name
                // Todo:refactor
                if res then
                    let succeeded, tmp = Conversions.convert routeVal paramType
                    if succeeded then
                        value <- tmp
                    succeeded
                else
                    false


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type FrameworkObjectsValueProvider () = 

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 
                let paramTypeDef = 
                    if paramType.IsGenericType then paramType.GetGenericTypeDefinition() else paramType

                if paramTypeDef == typeof<PropertyBag> || paramTypeDef == typedefof<PropertyBag<_>> then
                    value <- Activator.CreateInstance(paramType)
                    true
                else

                    // if (paramType.IsPrimitive) then 
                        // _request.Params.[name]
                        // false
                    false


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type RequestBoundValueProvider [<ImportingConstructor>] (request:HttpRequestBase) = 

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 
                let reqVal = request.Params.[name]

                if (reqVal == null) then 
                    false
                else
                    let succeeded, tmp = Conversions.convert reqVal paramType
                    if succeeded then
                        value <- tmp
                    succeeded


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type SerializerValueProvider [<ImportingConstructor>] (request:HttpRequestBase) = 
        let mutable _resolver = Unchecked.defaultof<ModelSerializerResolver>
        let mutable _contentNeg = Unchecked.defaultof<ContentNegotiator>

        [<Import>]
        member x.ModelSerializerResolver with set v = _resolver <- v

        [<Import>]
        member x.ContentNegotiator with set v = _contentNeg <- v

        interface IParameterValueProvider with
            member x.TryGetValue(name:string, paramType:Type, value:obj byref) = 

                let contentType = request.ContentType

                if String.IsNullOrEmpty contentType then
                    false
                else
                    let mime = _contentNeg.ResolveContentType contentType
                    let modelType = 
                        if paramType.IsGenericType then paramType.GetGenericArguments() |> Seq.head else paramType

                    let createGenMethod = typeof<ModelSerializerResolver>.GetMethod("CreateSerializer")
                    let serializerType = typedefof<IModelSerializer<_>>.MakeGenericType([|modelType|]) 
                    let deserializeMethod = serializerType.GetMethod "Deserialize"
                
                    let instMethod = createGenMethod.MakeGenericMethod( [|modelType|] )
                    let serializer = instMethod.Invoke(_resolver, [|mime|] )
                
                    if serializer != null then
                        let model = deserializeMethod.Invoke(serializer, [|""; contentType; request|])
                        value <- Activator.CreateInstance (paramType, [|model|])
                        true
                    else 
                        false




