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


    [<Export(typeof<IFilterActivator>)>]
    [<ExportMetadata("Order", Int32.MaxValue)>]
    type ReflectionBasedFilterActivator() = 
        interface IFilterActivator with
            member x.Activate(filterType) = 
                Activator.CreateInstance(filterType) :?> 'a


    [<AbstractClass;AllowNullLiteral>]
    type FilterDescriptorProvider() = 
        abstract member GetDescriptors : context:ActionExecutionContext -> FilterDescriptor seq


    /// Aggregates the FilterDescriptorProvider, sort/filter the results
    [<Export>]
    type FilterProvider 
        [<ImportingConstructor>]
        ( [<ImportMany>] descriptorProviders:Lazy<FilterDescriptorProvider, IComponentOrder> seq) = 

        let _ordered = Helper.order_lazy_set descriptorProviders

        member x.Provide<'TFilter when 'TFilter : null> (activator:IFilterActivator, context:ActionExecutionContext) : 'TFilter seq = 
            
            let descriptors = _ordered |> Seq.collect (fun dp -> dp.Force().GetDescriptors(context) )
            
            if not <| Seq.isEmpty descriptors then
                let descriptorsThatApply = 
                    descriptors 
                    |> Seq.filter (fun d -> d.Applies<'TFilter>()) 
                    |> Seq.sortBy (fun d -> d.Order)
                
                // get the list of skip filters
                let skippers = 
                    descriptors |> Seq.filter (fun d -> match d with | Skip _ -> true | _ -> false)

                // apply them
                let prunedList =
                    if not <| Seq.isEmpty skippers then  
                        descriptorsThatApply
                        |> Seq.filter (fun d -> not (skippers |> Seq.exists (fun skipper -> skipper.Rejects d)))
                    else
                        descriptorsThatApply

                // list of final filters that apply
                prunedList |> Seq.map (fun d -> d.Create<'TFilter>(activator) )
            else
                Seq.empty


    [<Export(typeof<FilterDescriptorProvider>)>]
    [<ExportMetadata("Order", 10000)>]
    type RouteScopeFilterProvider() =
        inherit FilterDescriptorProvider()

        override x.GetDescriptors(context) = 
            let route = context.RouteMatch.Route
            let res, value = route.ExtraData.TryGetValue(Constants.MR_Filters_Key)
            if res then value :?> FilterDescriptor seq
            else Seq.empty


    [<Export(typeof<FilterDescriptorProvider>)>]
    [<ExportMetadata("Order", 20000)>]
    type ControllerLevelFilterProvider() = 
        inherit FilterDescriptorProvider()

        override x.GetDescriptors(context) = 
            let res, value = context.ControllerDescriptor.Metadata.TryGetValue(Constants.MR_Filters_Key)
            if res then value :?> FilterDescriptor seq 
            else Seq.empty
      

    [<Export(typeof<FilterDescriptorProvider>)>]
    [<ExportMetadata("Order", 30000)>]
    type ActionLevelFilterProvider() = 
        inherit FilterDescriptorProvider()

        override x.GetDescriptors(context) = 
            let res, value = context.ActionDescriptor.Metadata.TryGetValue(Constants.MR_Filters_Key)
            if res then value :?> FilterDescriptor seq
            else Seq.empty








