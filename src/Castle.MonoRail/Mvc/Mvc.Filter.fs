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

namespace Castle.MonoRail.Filter

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
    open Castle.MonoRail.Hosting.Mvc.Typed

    type ExecuteWhen = 
        | Undefined = 0
        | Before = 1
        | After = 2

    [<Interface>]
    type IFilter =
        abstract member Execute : controller:obj * context:HttpContextBase -> bool

    type FilterDescriptor(filterType:Type, execWhen:ExecuteWhen) =
        let _type = filterType
        let _when = execWhen

        member this.Type = _type
        member this.ExecuteWhen = _when

    [<Interface>]
    type IFilterSelector = 
        abstract member Discover : execWhen:ExecuteWhen * context:ActionExecutionContext -> Type seq

    [<Interface>]
    type IFilterActivator = 
        abstract member Create : filter:Type -> IFilter

    [<Export(typeof<IFilterSelector>)>]
    type RouteScopeFilterSelector() =

        interface IFilterSelector with 
            member this.Discover (execWhen:ExecuteWhen, context:ActionExecutionContext) : Type seq =
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
            member this.Create (filter:Type) : IFilter =
                System.Activator.CreateInstance(filter) :?> IFilter

    type BaseFilterProcessor(execWhen:ExecuteWhen) = 
        inherit BaseActionProcessor()
        let _execWhen = execWhen
        let mutable _selectors : IFilterSelector seq = null
        let mutable _activators : IFilterActivator seq = null

        let discover_filters context =
            let types = List<Type>()

            _selectors |> Seq.iter (fun sel -> types.AddRange(sel.Discover(_execWhen, context)))

            types

        let activate filterType =
            Helpers.traverseWhileNull _activators (fun p -> p.Create(filterType))

        let can_proceed (filterTypes:List<Type>) (context:ActionExecutionContext) = 
            not (filterTypes |> Seq.exists (fun ftype -> (activate ftype).Execute(context.Prototype.Instance, context.HttpContext) = false))

        [<ImportMany(AllowRecomposition=true)>]
        member this.Providers 
            with get() = _selectors and set v = _selectors <- v

        [<ImportMany(AllowRecomposition=true)>]
        member this.Activators 
            with get() = _activators and set v = _activators <- v                                    
        
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


    [<System.Runtime.CompilerServices.ExtensionAttribute>]
    module public ExtensionMethods = 

        [<System.Runtime.CompilerServices.ExtensionAttribute>]
        let SetFilter<'a when 'a :> IFilter>(route:Route, execWhen:ExecuteWhen) = 
            if not (route.ExtraData.ContainsKey(Constants.MR_Filters_Key)) then
                route.ExtraData.[Constants.MR_Filters_Key] <- List<FilterDescriptor>()

            let descriptors = route.ExtraData.[Constants.MR_Filters_Key] :?> List<FilterDescriptor>

            descriptors.Add(FilterDescriptor(typeof<'a>, execWhen))

            route