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

namespace Castle.MonoRail

    open System
    open System.Reflection
    open System.Collections.Generic
    open System.Linq
    open Castle.MonoRail.OData
    open Castle.MonoRail.OData.Internal
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Csdl


    type internal SubControllerWrapper(entType:Type, creator:Func<ControllerCreationContext, ControllerPrototype>) = 

        do
            // ControllerCreationContext(routematch, context)
            ()

        member x.TargetType = entType

        member x.TypesMentioned : seq<Type> = Seq.empty

        member x.GetFunctionImports (edmModel:IEdmModel) : seq<IEdmFunctionImport> = 
            let container = edmModel.EntityContainers().ElementAt(0)
            let x = EdmFunctionImport(container, "testFun", EdmCoreModel.Instance.GetString(false))
            seq [ yield upcast x ]
            
        member x.Invoke(action:string, isColl:bool, parameters:(Type*obj) seq, value:obj, isOptional:bool)  = 
                
            true, null

