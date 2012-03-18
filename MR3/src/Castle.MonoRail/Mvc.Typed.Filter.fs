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

    // Providers of filters:
    // Route, ControllerDescriptor, ActionDescriptor - any other?

    // Functionality: SkipFilter( All, FilterType = typeof(FilterImpl) )

    // FilterProvider
    //   RouteFilterProvider
    //   FilterAttributeProvider

    // Customization of FilterProviders?
    // [HandleException] <- controller/action level
    // [OAuth] <- controller level
    // [SamlClaimCheck("claimId")]
    // [ValidateAuthentication]


    [<AbstractClass;AllowNullLiteral>]
    type FilterProvider() = 
        class 
            abstract member Create : unit -> unit
        end

    [<Export(typeof<FilterProvider>)>]
    type RouteScopeFilterProvider() =
        (* 
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

        override this.Create (filterInterface, context) =
            let route = context.RouteMatch.Route
            if route.ExtraData.ContainsKey(Constants.MR_Filters_Key) then
                let candidantes = route.ExtraData.[Constants.MR_Filters_Key] :?> FilterDescriptor seq
                candidantes 
                |> Seq.filter (fun c -> is_match c filterInterface context) 
                |> Seq.map (fun c -> c.Type) 
            else
                Seq.empty
        *()


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