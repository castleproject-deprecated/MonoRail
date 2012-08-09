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

namespace Castle.MonoRail.Extension.OData

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Data.Services.Common
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection
    open Castle.MonoRail.OData
    open Castle.MonoRail.Extension.OData
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed


    [<AllowNullLiteral>]
    type ControllerActionOperation(rt:ResourceType, actionName:string, isColl:bool, returnType:Type) = 
        let mutable _returnResType : ResourceType = null

        member x.ResourceType = rt
        member x.Name = actionName
        member x.ReturnsCollection = isColl
        member x.ReturnType = returnType
        member x.ReturnResourceType with get() = _returnResType and set(v) = _returnResType <- v


    type internal SubControllerInfo = {
        creator : Func<ControllerPrototype>;
        desc : TypedControllerDescriptor;
        mutable actions : ControllerActionOperation seq;
    } with 
        static member Empty = { creator = null; desc = null; actions = Seq.empty }