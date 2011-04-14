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

namespace Castle.MonoRail.Hosting

    open System.Web
    open System.ComponentModel.Composition

    [<AbstractClass>]
    type ComposableHandler() as self =

        abstract member ProcessRequest : request:HttpContextBase -> unit
        
        interface IHttpHandler with
            member this.ProcessRequest(context:HttpContext) : unit =
                let ctxWrapped = HttpContextWrapper(context)

                let req_container = Container.CreateRequestContainer(ctxWrapped)
                Container.SubscribeToRequestEndToDisposeContainer(context.ApplicationInstance)
                req_container.ComposeParts([this])

                self.ProcessRequest(ctxWrapped);

            member this.IsReusable = 
                true

        interface IComposableHandler with
            // funny way to define abstract members associated with interfaces
            member x.ProcessRequest (request:HttpContextBase) : unit = self.ProcessRequest(request)
        

