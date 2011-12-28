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

namespace Castle.MonoRail.Framework

    open System

    type public ComponentScope =
    | Application = 0
    | Request = 1
    // PartMetadata is used to put components in the request of app scope, ie.
    // PartMetadata("Scope", ComponentScope.Application)

    [<Interface; AllowNullLiteral>]
    type public IComponentOrder = 
        abstract member Order : int


    module Helper = 

        let internal order_lazy_set (set:Lazy<'a, IComponentOrder> seq) = 
            System.Linq.Enumerable.OrderBy(set, (fun e -> e.Metadata.Order)) :> Lazy<'a, IComponentOrder> seq
