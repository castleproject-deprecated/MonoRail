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

namespace Castle.MonoRail.Helpers

    open System
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.IO
    open System.Text
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines
    open Castle.MonoRail.Hosting

    [<AbstractClass>]
    type SerializerBasedHelper(context) = 
        inherit BaseHelper(context)

        let mutable _modelSerializerResolver : IModelSerializerResolver = null

        [<Import>]
        member x.ModelSerializer 
            with get() = _modelSerializerResolver
             and set(v) = _modelSerializerResolver <- v





