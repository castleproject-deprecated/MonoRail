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
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility

    type FilterDescriptor(filterType:Type, execWhen:ExecuteWhen) =
        member this.Type = filterType
        member this.ExecuteWhen = execWhen

    [<Interface>]
    type IFilterSelector = 
        abstract member Discover : execWhen:ExecuteWhen * context:ActionExecutionContext -> Type seq

    [<Interface>]
    type IFilterActivator = 
        abstract member Create : filter:Type -> IFilter

    [<Export(typeof<IFilterSelector>)>]
    type RouteScopeFilterSelector() =

        interface IFilterSelector with 
            member this.Discover (execWhen:ExecuteWhen, context:ActionExecutionContext) =
                let route = context.RouteMatch.Route

                if route.ExtraData.ContainsKey(Constants.MR_Filters_Key) then
                    let candidantes = route.ExtraData.[Constants.MR_Filters_Key] :?> FilterDescriptor seq
                    candidantes |> Seq.filter (fun c -> c.ExecuteWhen = execWhen) |> Seq.map (fun c -> c.Type) 
                else
                    Seq.empty


    [<Export(typeof<IFilterActivator>)>]
    [<ExportMetadata("Order", 100000)>]
    type ReflectionBasedFilterActivator() =

        interface IFilterActivator with
            member this.Create (filter:Type) =
                System.Activator.CreateInstance(filter) :?> IFilter


    type BaseFilterProcessor(execWhen:ExecuteWhen) = 
        inherit BaseActionProcessor()
        let mutable _selectors : IFilterSelector seq = Seq.empty
        let mutable _activators : Lazy<IFilterActivator, IComponentOrder> seq = Seq.empty

        let discover_filters context =
            let types = List<Type>()

            _selectors |> Seq.iter (fun sel -> types.AddRange(sel.Discover(execWhen, context)))

            types

        let activate filterType =
            Helpers.traverseWhileNull _activators (fun p -> p.Value.Create(filterType))

        let can_proceed (filterTypes:List<Type>) (context:ActionExecutionContext) = 
            not (filterTypes |> Seq.exists (fun ftype -> (activate ftype).Execute(context.Prototype.Instance, context.HttpContext) = false))

        [<ImportMany(AllowRecomposition=true)>]
        member this.Providers 
            with get() = _selectors and set v = _selectors <- v

        [<ImportMany(AllowRecomposition=true)>]
        member this.Activators 
            with get() = _activators and set v = _activators <- Helper.order_lazy_set v                                    
        
        override this.Process(context:ActionExecutionContext) = 
            let filtersTypes = discover_filters context
                    
            if can_proceed filtersTypes context then
                this.NextProcess(context)


    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_BeforeActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type BeforeActionFilterProcessor() =
        inherit BaseFilterProcessor(ExecuteWhen.Before)

    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_AfterActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type AfterActionFilterProcessor() =
        inherit BaseFilterProcessor(ExecuteWhen.After)
