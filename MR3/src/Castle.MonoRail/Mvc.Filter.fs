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

namespace Castle.MonoRail

    open System
    open System.Web
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Castle.MonoRail.Hosting.Mvc

    type internal IFilterWithActionResult = 
        interface 
            abstract member ActionResult : ActionResult with get
        end

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

        interface IFilterWithActionResult with member x.ActionResult = x.ActionResult


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

        interface IFilterWithActionResult with member x.ActionResult = x.ActionResult


    /// <summary>
    /// Invoked before any filter and before the action is run. Gives a chance 
    /// to validate the session authentication state and authorization grant for the action. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    [<Interface;AllowNullLiteral>]
    type IAuthorizationFilter = 
        // inherit IProcessFilter
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
        // inherit IProcessFilter
        abstract member BeforeAction : context:PreActionFilterExecutionContext -> unit
        abstract member AfterAction  : context:AfterActionFilterExecutionContext -> unit


    /// <summary>
    /// Invoked when an exception happens during action processing. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    [<Interface;AllowNullLiteral>]
    type IExceptionFilter = 
        // inherit IProcessFilter
        abstract member HandleException : context:ExceptionFilterExecutionContext -> unit
        


