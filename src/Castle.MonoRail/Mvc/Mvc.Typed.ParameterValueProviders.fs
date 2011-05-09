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
        let _route_match = route_match

        interface IParameterValueProvider with
            member x.TryGetValue(paramType:ActionParameterDescriptor, value:obj byref) = 
                value <- null
                false


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 100000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type RequestBoundValueProvider [<ImportingConstructor>] (request:HttpRequestBase) = 
        let _request = request

        interface IParameterValueProvider with
            member x.TryGetValue(paramType:ActionParameterDescriptor, value:obj byref) = 
                value <- null
                // if (paramType.IsPrimitive) then 
                    // _request.Params.[name]
                    // false
                false


    [<Export(typeof<IParameterValueProvider>)>]
    [<ExportMetadata("Order", 1000)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type PostValueProvider [<ImportingConstructor>] (request:HttpRequestBase) = 
        let _request = request
        let mutable _resolver = Unchecked.defaultof<ModelSerializerResolver>

        [<Import>]
        member x.ModelSerializerResolver with set v = _resolver <- v

        interface IParameterValueProvider with
            member x.TryGetValue(paramType:ActionParameterDescriptor, value:obj byref) = 
                // if paramType.IsGenericType && paramType.GetGe
                
                value <- null


                // _resolver.

                // if (paramType.IsPrimitive) then 
                    // _request.Params.[name]
                    // false
                false




