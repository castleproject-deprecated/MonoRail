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

namespace Castle.MonoRail.OData.Internal

    open System
    open System.Reflection
    open System.Collections.Generic
    open System.Linq
    open Castle.MonoRail.OData
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl


    [<Interface>]
    type IEdmReflectionTypeAccessor = 
        abstract member TargetType : System.Type with get

    type TypedEdmEntityType(namespace_, name, typ:Type) = 
        inherit EdmEntityType(namespace_, name)

        member x.TargetType = typ

        interface IEdmReflectionTypeAccessor with
            member x.TargetType = typ


    type TypedEdmComplexType(namespace_, name, typ:Type) = 
        inherit EdmComplexType(namespace_, name)

        member x.TargetType = typ

        interface IEdmReflectionTypeAccessor with
            member x.TargetType = typ

        


