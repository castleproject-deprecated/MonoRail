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

namespace Castle.MonoRail

    open System.Web
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed

    type ActionResultContext
        (action:ControllerActionDescriptor, 
         controller:ControllerDescriptor, controllerprot:ControllerPrototype, 
         httpctx:HttpContextBase, serv:IServiceRegistry) = 

        let _action = action
        let _controllerdesc = controller
        let _controllerprot = controllerprot
        let _httpctx = httpctx
        let _serv = serv

        member x.HttpContext = _httpctx 
        member x.ServiceRegistry = serv
        member x.ActionDescriptor = _action
        member x.ControllerDescriptor = _controllerdesc
        member x.Prototype = _controllerprot


    [<AbstractClass>]
    type public ActionResult() =
        abstract member Execute : ActionResultContext -> unit

