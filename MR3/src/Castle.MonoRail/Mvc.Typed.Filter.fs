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
    open System.Runtime.InteropServices

    // Functionality: SkipFilter( All, FilterType = typeof(FilterImpl) )
    // Customization of FilterProviders?
    // [HandleException] <- controller/action level
    // [OAuth] <- controller level
    // [SamlClaimCheck("claimId")]
    // [ValidateAuthentication]


    type FilterDescriptor(target:obj, order:int) = 

        member x.Supports<'TFilter>() = 
            (target :? 'TFilter)

        member x.CreateFilter(activator) = 
            if target :? 'a then
                target :?> 'a
            else
                null


    [<AllowNullLiteral>]
    type IFilterActivator =
        interface
            abstract member Activate : filterType : Type -> 'a
        end

    [<AbstractClass;AllowNullLiteral>]
    [<InheritedExport(typeof<FilterProvider>)>]
    type FilterProvider() = 
        abstract member GetDescriptors : context:ActionExecutionContext -> FilterDescriptor []

        member x.Provide<'TFilter when 'TFilter : null> (activator:IFilterActivator, context:ActionExecutionContext) : 'TFilter seq = 
            let descriptors = x.GetDescriptors(context)
            if descriptors <> null then
                descriptors 
                |> Array.filter (fun d -> d.Supports() ) 
                |> Seq.map (fun d -> d.CreateFilter(activator) )
            else
                Seq.empty


    type ControllerLevelFilterProvider() = 
        inherit FilterProvider()

        override x.GetDescriptors(context) = 
            let res, value = context.ControllerDescriptor.Metadata.TryGetValue("action.filter")
            if res then value :?> FilterDescriptor []
            else Array.empty
      

    type ActionLevelFilterProvider() = 
        inherit FilterProvider()

        override x.GetDescriptors(context) = 
            let res, value = context.ActionDescriptor.Metadata.TryGetValue("action.filter")
            if res then value :?> FilterDescriptor []
            else Array.empty


    type RouteScopeFilterProvider() =
        inherit FilterProvider()

        override x.GetDescriptors(context) = 
            let route = context.RouteMatch.Route
            let res, value = route.ExtraData.TryGetValue(Constants.MR_Filters_Key)
            if res then value :?> FilterDescriptor []
            else Array.empty







