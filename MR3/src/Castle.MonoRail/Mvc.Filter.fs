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

    [<AllowNullLiteral>]
    type IFilterActivator =
        interface
            abstract member Activate : filterType : Type -> 'TFilter when 'TFilter : null
        end

    type FilterDescriptor = 
        | IncludeInstance of obj * int
        | IncludeType of Type * int * (obj -> unit)
        | Skip of Type
        with 
            member x.Order = 
                match x with 
                | IncludeInstance (_, order) -> order
                | IncludeType (_, order, _) -> order
                | _ -> 0

            member x.Create<'TFilter when 'TFilter : null>(activator:IFilterActivator) : 'TFilter = 
                match x with 
                | IncludeInstance (instance, _) -> 
                    Helpers.arg_not_null instance "instance"
                    instance :?> 'TFilter
                | IncludeType (filterType, _, configurer) -> 
                    Helpers.assumes_concrete filterType
                    let instance = activator.Activate<'TFilter>(filterType)
                    if configurer != null then configurer(instance)
                    instance
                | _ -> failwith "We can't activate a skip filter descriptor"

            member x.Applies<'TFilter>() = 
                match x with 
                | IncludeInstance (instance, _) -> instance :? 'TFilter 
                | IncludeType (filterType, _, _) -> typeof<'TFilter>.IsAssignableFrom(filterType)
                | _ -> false
            
            member x.Rejects(descriptor:FilterDescriptor) = 
                match x with 
                | Skip filterTypeToSkip -> 
                    match descriptor with 
                    | IncludeInstance (instance, _) -> filterTypeToSkip = instance.GetType()
                    | IncludeType (filterType, _, _) -> filterTypeToSkip = filterType
                    | _ -> false
                | _ -> false


    type internal IFilterDescriptorBuilder = 
        interface 
            abstract member Create : unit -> FilterDescriptor
        end


namespace Castle.MonoRail

    open System
    open System.Web
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Castle.MonoRail.Hosting.Mvc

    [<AbstractClass;AllowNullLiteral>]
    type FilterExecutionContext (actionDescriptor, controller, httpContext, routeMatch, exc) = 
        inherit ActionExecutionContext (actionDescriptor, controller, httpContext, routeMatch)

        do
            if exc <> null then
                base.Exception <- exc

        new (context:ActionExecutionContext) = 
            FilterExecutionContext(context.ActionDescriptor, context.Prototype, context.HttpContext, context.RouteMatch, context.Exception)


    type PreActionFilterExecutionContext = 
        inherit FilterExecutionContext
        [<DefaultValue>] val mutable private _actionResult : ActionResult

        new (context) = 
            { inherit FilterExecutionContext(context) }
        new (actionDescriptor, controller, httpContext, routeMatch) = 
            { inherit FilterExecutionContext(actionDescriptor, controller, httpContext, routeMatch, null) }

        /// If a filter decides to take over and return a specific action (say redirecting)
        member x.ActionResult with get() = x._actionResult and set(v) = x._actionResult <- v


    type AfterActionFilterExecutionContext  = 
        inherit FilterExecutionContext

        new (context) = 
            { inherit FilterExecutionContext(context) }
        new (actionDescriptor, controller, httpContext, routeMatch) = 
            { inherit FilterExecutionContext(actionDescriptor, controller, httpContext, routeMatch, null) }


    type ExceptionFilterExecutionContext = 
        inherit FilterExecutionContext
        [<DefaultValue>] val mutable private _actionResult : ActionResult

        new (context) = 
            { inherit FilterExecutionContext(context) }
        new (actionDescriptor, controller, httpContext, routeMatch, exc) = 
            { inherit FilterExecutionContext(actionDescriptor, controller, httpContext, routeMatch, exc) }

        /// If a filter decides to take over and return a specific action (say redirecting)
        member x.ActionResult with get() = x._actionResult and set(v) = x._actionResult <- v


    /// <summary>
    /// Invoked before any filter and before the action is run. Gives a chance 
    /// to validate the session authentication state and authorization grant for the action. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    [<Interface;AllowNullLiteral>]
    type IAuthorizationFilter = 
        abstract member AuthorizeRequest : context:PreActionFilterExecutionContext -> unit

    /// <summary>
    /// Happens around an action processing (before/after). 
    /// </summary>
    /// <remarks>
    /// AfterAction is guaranted to be executed, even if the action throws 
    /// (unless it's system level exception that corrupts the runtime)
    /// </remarks>
    [<Interface;AllowNullLiteral>]
    type IActionFilter = 
        abstract member BeforeAction : context:PreActionFilterExecutionContext -> unit
        abstract member AfterAction  : context:AfterActionFilterExecutionContext -> unit

    /// <summary>
    /// Invoked when an exception happens during action processing. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    [<Interface;AllowNullLiteral>]
    type IExceptionFilter = 
        abstract member HandleException : context:ExceptionFilterExecutionContext -> unit
        
    
    /// Inherit from this attribute to define a filter that implements 
    /// one of more the the filter interfaces. 
    [<AbstractClass;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Method, Inherited = true)>]
    type FilterAttribute() = 
        inherit Attribute() 
        let mutable _order = 0
        member x.Order with get() = _order and set(v) = _order <- v
        
        interface IFilterDescriptorBuilder with 
            member x.Create() = 
                FilterDescriptor.IncludeInstance (x, _order)


    /// Inherit from this attribute to define filters that delegate their implementation 
    /// to a different type (in other words, they are not in itself the filter implementation)
    /// This is particularly useful for integration with IoC Containers since the filter 
    /// type will be created by using the IFilterActivator implementation
    [<AbstractClass;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Method, Inherited = true)>]
    type FilterTypeAttribute(filterType:Type) = 
        inherit Attribute() 
        let mutable _order = 0
        member x.Order with get() = _order and set(v) = _order <- v

        /// Override to give the filter instance additional configuration parameters 
        abstract member Configure : filterInstance:obj -> unit 
        
        interface IFilterDescriptorBuilder with 
            member x.Create() = 
                FilterDescriptor.IncludeType (filterType, _order, fun ins -> x.Configure(ins) )
    

    /// Use this attribute to skip the execution of a particular filter type.
    /// This can be subclassed for better semantic API.
    [<AttributeUsage(AttributeTargets.Class|||AttributeTargets.Method, Inherited = true)>]
    type SkipFilterAttribute(filterType:Type) = 
        inherit Attribute() 
        member x.FilterType = filterType
        
        interface IFilterDescriptorBuilder with 
            member x.Create() = 
                FilterDescriptor.Skip (filterType)
