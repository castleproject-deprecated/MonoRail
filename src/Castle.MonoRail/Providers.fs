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

namespace Castle.MonoRail.Hosting.Mvc

    open System.Web
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extensibility


    [<ControllerProviderExport(9000000)>]
    type ReflectionBasedControllerProvider() =
        inherit ControllerProvider()

        override this.Create(data:RouteData, context:HttpContextBase) : ControllerPrototype = 
            Unchecked.defaultof<ControllerPrototype>



    [<ControllerProviderExport(8000000)>]
    type MefControllerProvider() =
        inherit ControllerProvider()

        override this.Create(data:RouteData, context:HttpContextBase) : ControllerPrototype = 
            Unchecked.defaultof<ControllerPrototype>



    [<ControllerExecutorProviderExport(9000000)>]
    type PocoControllerExecutorProvider() =
        inherit ControllerExecutorProvider()

        override this.Create(prototype:ControllerPrototype, data:RouteData, context:HttpContextBase) : ControllerExecutor = 
            Unchecked.defaultof<ControllerExecutor>
    




