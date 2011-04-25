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

module Helpers
    
    open System
    open System.Collections.Generic
    open System.Linq
    open System.Reflection

    let inline (==) a b = Object.ReferenceEquals(a, b)
    let inline (!=) a b = not (Object.ReferenceEquals(a, b))

    let internal guard_load_types (asm:Assembly) =
        try
            asm.GetTypes()
        with
        | :? ReflectionTypeLoadException as exn -> 
            exn.Types

    // produces non-null seq of types
    let typesInAssembly (asm:Assembly) f =
        seq { 
                let types = guard_load_types(asm)
                for t in types do
                    if (t <> null && f(t)) then 
                        yield t
            }


