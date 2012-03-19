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

namespace Castle.MonoRail.Hosting.Mvc.Typed
    
    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility


    [<Export(typeof<ITypeDescriptorBuilderContributor>)>]
    [<Export(typeof<IActionDescriptorBuilderContributor>)>]
    [<ExportMetadata("Order", 20000);AllowNullLiteral>]
    type ControllerAndActionFilterDescriptorBuilder() = 

        let build_descriptor (att:obj) = 
            let fatt = att :?> FilterAttribute
            FilterDescriptor(fatt, fatt.Order)

        let build_descriptors (attrs:obj[]) = 
            attrs |> Array.map build_descriptor

        interface ITypeDescriptorBuilderContributor with            
            member x.Process(controllerType, desc) = 
                let attProvider = controllerType |> box :?> ICustomAttributeProvider 
                let attrs = attProvider.GetCustomAttributes(typeof<FilterAttribute>, true)
                if attrs.Length > 0 then 
                    desc.Metadata.["action.filters"] <- build_descriptors(attrs) 

        interface IActionDescriptorBuilderContributor with
            member x.Process(actionDesc, controllerDesc) = 
                let attProvider = actionDesc |> box :?> ICustomAttributeProvider 
                let attrs = attProvider.GetCustomAttributes(typeof<FilterAttribute>, true)
                if attrs.Length > 0 then 
                    actionDesc.Metadata.["action.filters"] <- build_descriptors(attrs) 
                


