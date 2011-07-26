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

namespace Castle.MonoRail.Helpers

    open System
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines
    

    type public ViewComponentHelper(ctx) =
        inherit BaseHelper(ctx)

        member this.Render<'tvc when 'tvc :> IViewComponent>() =
            let executor = this.ServiceRegistry.ViewComponentExecutor
            executor.Execute(typeof<'tvc>.Name, this.HttpContext, null)

        member this.Render<'tvc when 'tvc :> IViewComponent>(configurer:Action<'tvc>) =
            let executor = this.ServiceRegistry.ViewComponentExecutor
            executor.Execute(typeof<'tvc>.Name, this.HttpContext, configurer)