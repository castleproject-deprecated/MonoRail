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
    open Castle.MonoRail.Extensibility
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Helpers

    type ActionExecutionContext
        (action:ControllerActionDescriptor, controller:ControllerDescriptor) = 
        let _action = action
        let _controller = controller
        let _parameters = lazy ( 
                let dict = Dictionary<string,obj>() 
                for pair in _action.Parameters do
                    dict.[pair.Name] <- null
                dict
            )
        
        member x.Action = _action
        member x.Controller = _controller
        member x.Parameters = _parameters.Force()


        // httpcontext?

    // IParameterValueProvider
    //   Forms, QS, Cookies, Binder?, FxValues?

    [<Interface>]
    type IActionProcessor = 
        abstract Next : IActionProcessor with get, set
        abstract Process : context:ActionExecutionContext -> unit

    [<AbstractClass>]
    type BaseActionProcessor() as self = 
        let mutable _next = Unchecked.defaultof<IActionProcessor>

        abstract Process : context:ActionExecutionContext -> unit

        interface IActionProcessor with
            member x.Next
                with get() = _next and set v = _next <- v
            member x.Process(context:ActionExecutionContext) = self.Process(context)
    
    [<Export(typeof<IActionProcessor>)>]
    [<ExportMetadata("Order", 100000)>]
    type ActionExecutorProcessor() = 
        inherit BaseActionProcessor()

        // IValueProvider

        override x.Process(context:ActionExecutionContext) = 
            
            

            ignore()
