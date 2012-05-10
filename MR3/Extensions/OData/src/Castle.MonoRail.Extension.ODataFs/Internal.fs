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

module InternalUtils 

    open System
    open System.Reflection
    open System.Collections.Generic


    let getEnumerableElementType (possibleEnumerableType:Type) = 
        let found = possibleEnumerableType.FindInterfaces(TypeFilter(fun t o -> (o :?> Type).IsAssignableFrom(t)), typedefof<IEnumerable<_>>) 
        if found.Length = 0
        then None
        else Some(found.[0].GetGenericArguments().[0])
