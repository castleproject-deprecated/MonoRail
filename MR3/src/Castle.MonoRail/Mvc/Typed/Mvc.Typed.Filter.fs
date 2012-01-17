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

    [<AllowNullLiteral>]
    type FilterDescriptor(filterType:Type) =
        member this.Type = filterType

    [<AllowNullLiteral>]
    type ExceptionFilterDescriptor(filter:Type, excption:Type) =
        inherit FilterDescriptor(filter)
        member this.Exception = excption

    [<Interface;AllowNullLiteral>]
    type IFilterProvider = 
        abstract member Discover : filterInterface:Type * context:ActionExecutionContext -> Type seq

    [<Interface;AllowNullLiteral>]
    type IFilterActivator = 
        abstract member CreateFilter : filter:Type * context:HttpContextBase -> 'a

    [<Export(typeof<IFilterProvider>)>]
    type RouteScopeFilterProvider() =

        let is_match (d:FilterDescriptor) (t:Type) (c:ActionExecutionContext) =
            if t.IsAssignableFrom d.Type then
                if d.GetType() = typeof<ExceptionFilterDescriptor> then
                    if c.Exception = null then
                        false
                    else
                        // this is order sensitive and likely to cause problems
                        (d :?> ExceptionFilterDescriptor).Exception.IsAssignableFrom(c.Exception.GetType())
                else
                    true
            else
                false

        interface IFilterProvider with 
            member this.Discover (filterInterface:Type, context:ActionExecutionContext) =
                let route = context.RouteMatch.Route
                if route.ExtraData.ContainsKey(Constants.MR_Filters_Key) then
                    let candidantes = route.ExtraData.[Constants.MR_Filters_Key] :?> FilterDescriptor seq
                    candidantes 
                    |> Seq.filter (fun c -> is_match c filterInterface context) 
                    |> Seq.map (fun c -> c.Type) 
                else
                    Seq.empty


    [<Export(typeof<IFilterActivator>)>]
    [<ExportMetadata("Order", 100000)>]
    type ReflectionBasedFilterActivator() =
        interface IFilterActivator with
            member x.CreateFilter (filter:Type, context:HttpContextBase) : 'a =
                System.Activator.CreateInstance(filter) :?> 'a


    [<AbstractClass>]
    type BaseFilterProcessor<'a when 'a :> IActionFilter>(filterEx:'a -> FilterExecutionContext -> bool) = 
        inherit BaseActionProcessor()
        let mutable _providers : IFilterProvider seq = Seq.empty
        let mutable _activators : Lazy<IFilterActivator, IComponentOrder> seq = Seq.empty

        let activate (filterType:Type) (activators: Lazy<IFilterActivator, IComponentOrder> seq) (context:HttpContextBase) : 'a =
             Helpers.traverseWhileNull activators (fun p -> p.Value.CreateFilter(filterType, context))

        let discover_filters (context:ActionExecutionContext) : List<Type> =
            let types = List()
            _providers |> Seq.iter (fun sel -> types.AddRange(sel.Discover(typeof<'a>, context)))
            types

        [<ImportMany(AllowRecomposition=true)>]
        member this.Providers 
            with get() = _providers and set v = _providers <- v

        [<ImportMany(AllowRecomposition=true)>]
        member this.Activators 
            with get() = _activators and set v = _activators <- Helper.order_lazy_set v                                    
        
        override this.Process(context:ActionExecutionContext) = 
            let filtersTypes = discover_filters(context)
            
            if (Seq.isEmpty filtersTypes) then
                this.NextProcess(context)
            else 
                let filterCtx = FilterExecutionContext(context)

                let shouldProceed = 
                    filtersTypes 
                    |> Seq.choose (fun filterType -> (
                                                        let filter = activate filterType this.Activators context.HttpContext
                                                        let shouldProceed = (filterEx filter filterCtx)
                                                        if not shouldProceed then Some(false) else None
                                                     ))
                    |> Seq.isEmpty

                if shouldProceed then 
                    this.NextProcess(context)

    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_BeforeActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type BeforeActionFilterProcessor() =
        inherit BaseFilterProcessor<IBeforeActionFilter>((fun (filter) ctx -> filter.Execute(ctx)))


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_AfterActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type AfterActionFilterProcessor() =
        inherit BaseFilterProcessor<IAfterActionFilter>((fun filter ctx -> filter.Execute(ctx)))

    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ExecutionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ExceptionFilterProcessor() =
        inherit BaseFilterProcessor<IExceptionFilter>((fun filter ctx -> filter.Execute(ctx)))
