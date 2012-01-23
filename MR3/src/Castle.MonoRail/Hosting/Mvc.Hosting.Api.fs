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

namespace Castle.MonoRail.Hosting.Mvc

    open System
    open System.Web
    open System.Collections.Generic
    open Castle.MonoRail.Routing

    [<AbstractClass; AllowNullLiteral>]
    type ControllerProvider() = 
        abstract member Create : data:RouteMatch * context:HttpContextBase -> ControllerPrototype

    and [<AbstractClass; AllowNullLiteral>]
        ControllerExecutor() = 
            abstract member Execute : controller:ControllerPrototype * route_data:RouteMatch * context:HttpContextBase -> unit

            interface IDisposable with 
                override this.Dispose() = ()
    
    and [<AbstractClass; AllowNullLiteral>]
        ControllerExecutorProvider() = 
            abstract member Create : prototype:ControllerPrototype * data:RouteMatch * context:HttpContextBase -> ControllerExecutor

    and [<AllowNullLiteral>]
        ControllerPrototype(controller:obj) =
            let _meta = lazy Dictionary<string,obj>()
            member this.Metadata = _meta.Force() :> IDictionary<string,obj>
            member this.Instance = controller
