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

module Conversions

    open System
    open System.Reflection

    let internal convert (value:obj) (desiredType:Type) : bool * obj = 
        let mutable tmp = null
        match desiredType with
        | ptype when ptype = typeof<bool> ->
            // what shall we check for? '0'/'false' ?
            false, null
        | ptype when ptype = typeof<string> || ptype.IsPrimitive -> 
            tmp <- Convert.ChangeType(value, desiredType)
            true, tmp
        | ptype when ptype.IsEnum ->
            if value != null then 
                tmp <- Enum.Parse(desiredType, value.ToString())
                true, tmp
            else 
                false, null
        | _ -> 
            false, null
