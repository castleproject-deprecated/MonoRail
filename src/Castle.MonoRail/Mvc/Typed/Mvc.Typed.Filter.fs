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
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open System.Runtime.InteropServices

    type FilterDescriptor(filterType:Type) =
        member this.Type = filterType

    type ExceptionFilterDescriptor(filter:Type, excption:Type) =
        inherit FilterDescriptor(filter)

        member this.Exception = excption

    [<Interface>]
    type IFilterSelector = 
        abstract member Discover : filterSpec:Type * context:ActionExecutionContext -> Type seq

    [<Interface>]
    type IFilterActivator = 
        abstract member ActivateBeforeActionFilter : filter:Type * context:HttpContextBase -> IBeforeActionFilter
        abstract member ActivateAfterActionFilter : filter:Type * context:HttpContextBase -> IAfterActionFilter
        abstract member ActivateExceptionFilter : filter:Type * context:HttpContextBase -> IExceptionFilter

    [<Export(typeof<IFilterSelector>)>]
    type RouteScopeFilterSelector() =

        let is_match (d:FilterDescriptor) (t:Type) (c:ActionExecutionContext) =
            if t.IsAssignableFrom d.Type then
                if d.GetType() = typeof<ExceptionFilterDescriptor> then
                    if c.Exception = null then
                        false
                    else
                        (d :?> ExceptionFilterDescriptor).Exception.IsAssignableFrom(c.Exception.GetType())
                else
                    true
            else
                false

        interface IFilterSelector with 
            member this.Discover (filterSpec:Type, context:ActionExecutionContext) =
                let route = context.RouteMatch.Route

                if route.ExtraData.ContainsKey(Constants.MR_Filters_Key) then
                    let candidantes = route.ExtraData.[Constants.MR_Filters_Key] :?> FilterDescriptor seq

                    candidantes |> Seq.filter (fun c -> is_match c filterSpec context) |> Seq.map (fun c -> c.Type) 
                else
                    Seq.empty


    [<Export(typeof<IFilterActivator>)>]
    [<ExportMetadata("Order", 100000)>]
    type ReflectionBasedFilterActivator() =
        interface IFilterActivator with
            member this.ActivateBeforeActionFilter (filter:Type, context:HttpContextBase) =
                System.Activator.CreateInstance(filter) :?> IBeforeActionFilter

            member this.ActivateAfterActionFilter (filter:Type, context:HttpContextBase) =
                System.Activator.CreateInstance(filter) :?> IAfterActionFilter

            member this.ActivateExceptionFilter (filter:Type, context:HttpContextBase) =
                System.Activator.CreateInstance(filter) :?> IExceptionFilter

    [<Export()>]
    type FilterRegistry() =
        let mutable _selectors : IFilterSelector seq = Seq.empty

        member this.discover_filters (spec:Type, context:ActionExecutionContext) : List<Type> =
            let types = List<Type>()
            _selectors |> Seq.iter (fun sel -> types.AddRange(sel.Discover(spec, context)))
            types

        [<ImportMany(AllowRecomposition=true)>]
        member this.Selectors 
            with get() = _selectors and set v = _selectors <- v

        member this.ExecutionContextHasFilter<'a>(context:ActionExecutionContext) =
            _selectors |> Seq.exists (fun sel -> sel.Discover(typeof<'a>, context) |> Seq.length > 0)
            

    [<AbstractClass>]
    type BaseFilterProcessor() = 
        inherit BaseActionProcessor()
        let mutable _selectors : IFilterSelector seq = Seq.empty
        let mutable _activators : Lazy<IFilterActivator, IComponentOrder> seq = Seq.empty

        abstract member filterSpec: Type with get

        abstract member can_proceed: filterTypes:List<Type> * context:FilterExecutionContext -> bool

        member this.discover_filters (context:ActionExecutionContext) : List<Type> =
            let types = List<Type>()
            _selectors |> Seq.iter (fun sel -> types.AddRange(sel.Discover(this.filterSpec, context)))
            types

        [<ImportMany(AllowRecomposition=true)>]
        member this.Selectors 
            with get() = _selectors and set v = _selectors <- v

        [<ImportMany(AllowRecomposition=true)>]
        member this.Activators 
            with get() = _activators and set v = _activators <- Helper.order_lazy_set v                                    
        
        override this.Process(context:ActionExecutionContext) = 
            let filtersTypes = this.discover_filters(context)
                    
            if this.can_proceed (filtersTypes, FilterExecutionContext(context)) then
                this.NextProcess(context)


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_BeforeActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type BeforeActionFilterProcessor() =
        inherit BaseFilterProcessor()

        let activate (filterType:Type) (activators: Lazy<IFilterActivator, IComponentOrder> seq) (context:HttpContextBase) =
            Helpers.traverseWhileNull activators (fun p -> p.Value.ActivateBeforeActionFilter(filterType, context))

        override this.filterSpec with get() = typeof<IBeforeActionFilter>

        override this.can_proceed(filterTypes:List<Type>, context:FilterExecutionContext) : bool =
            not (filterTypes 
                  |> Seq.exists (fun ftype -> not ((activate ftype this.Activators context.HttpContext).Execute(context)) ))


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_AfterActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type AfterActionFilterProcessor() =
        inherit BaseFilterProcessor()

        let activate (filterType:Type) (activators: Lazy<IFilterActivator, IComponentOrder> seq) (context:HttpContextBase) =
            Helpers.traverseWhileNull activators (fun p -> p.Value.ActivateAfterActionFilter(filterType, context))

        override this.filterSpec with get() = typeof<IAfterActionFilter>

        override this.can_proceed(filterTypes:List<Type>, context:FilterExecutionContext) : bool =
            not (filterTypes 
                  |> Seq.exists (fun ftype -> not ((activate ftype this.Activators context.HttpContext).Execute(context)) ))


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ExecutionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ExceptionFilterProcessor() =
        inherit BaseFilterProcessor()

        let activate (filterType:Type) (activators: Lazy<IFilterActivator, IComponentOrder> seq) (context:HttpContextBase) =
            Helpers.traverseWhileNull activators (fun p -> p.Value.ActivateExceptionFilter(filterType, context))

        override this.filterSpec with get() = typeof<IExceptionFilter>

        override this.can_proceed(filterTypes:List<Type>, context:FilterExecutionContext) : bool =
            not (filterTypes 
                  |> Seq.exists (fun ftype -> not ((activate ftype this.Activators context.HttpContext).Execute(context)) ))
