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

    open System
    open System.Collections.Generic
    open System.Web


    type ResourceLink() = 
        [<DefaultValue>] val mutable _link : string
        [<DefaultValue>] val mutable _rel : string
        [<DefaultValue>] val mutable _label : string
        member x.Link 
            with get() = x._link and set(v) = x._link <- v
        member x.Rel
            with get() = x._rel and set(v) = x._rel <- v
        member x.Label 
            with get() = x._label and set(v) = x._label <- v


    type ResourceResult<'a>(resource:'a) = 
        inherit ActionResult()
        let _links = lazy List<ResourceLink>()

        member x.Links = _links.Force()

        override this.Execute(request:HttpContextBase, registry:IServiceRegistry) = 
            ignore()

