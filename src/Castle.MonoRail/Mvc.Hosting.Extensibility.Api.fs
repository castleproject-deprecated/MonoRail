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

namespace Castle.MonoRail.Hosting.Mvc.Extensibility

    open System
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Framework

    [<MetadataAttribute>]
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple=false)>]
    type public ControllerProviderExportAttribute(order:int) =
        inherit ExportAttribute(typeof<ControllerProvider>)
        let _order = order
        
        member x.Order = _order

    [<MetadataAttribute>]
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple=false)>]
    type public ControllerExecutorProviderExportAttribute(order:int) =
        inherit ExportAttribute(typeof<ControllerExecutorProvider>)
        let _order = order
        
        member x.Order = _order


    module Helper = 

        let internal order_lazy_set (set:IEnumerable<Lazy<'a, IComponentOrder>>) = 
            System.Linq.Enumerable.OrderBy(set, (fun e -> e.Metadata.Order)) :> IEnumerable<Lazy<'a, IComponentOrder>>

