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

[<AutoOpen>]
module Helpers
    
    open System
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open System.Dynamic

    let inline (==) a b = Object.ReferenceEquals(a, b)
    let inline (!=) a b = not (Object.ReferenceEquals(a, b))

    let internal guard_load_types (asm:Assembly) =
        try
            asm.GetTypes()
        with
        | :? ReflectionTypeLoadException as exn -> 
            exn.Types

    // produces non-null seq of types
    let internal typesInAssembly (asm:Assembly) f =
        seq { 
                let types = guard_load_types(asm)
                for t in types do
                    if (t <> null && f(t)) then 
                        yield t
            }

    let internal to_controller_name (typ:System.Type) = 
        let name = typ.Name
        if name.EndsWith "Controller" then
            name.Substring (0, name.Length - 10)
        else 
            name

    // see http://www.trelford.com/blog/post/Exposing-F-Dynamic-Lookup-to-C-WPF-Silverlight.aspx
    // this type is NOT thread safe and doesn't need to be
    type DynamicLookup() =
        inherit DynamicObject()
        // best perf if we use Dictionary 
        let mutable properties = Map.empty

        member private this.GetValue name = 
            Map.tryFind name properties

        member private this.SetValue (name,value) =
            properties <-
                properties 
                |> Map.remove name 
                |> Map.add name value

        override this.TryGetMember(binder:GetMemberBinder,result:obj byref) =     
            match this.GetValue binder.Name with
            | Some value -> result <- value; true
            | None -> false
    
        override this.TrySetMember(binder:SetMemberBinder, value:obj) =        
            this.SetValue(binder.Name,value)
            true
    
        override this.GetDynamicMemberNames() =
            properties |> Seq.map (fun pair -> pair.Key)
    
        static member (?) (lookup:#DynamicLookup,name:string) =
            match lookup.GetValue name with
            | Some(value) -> value
            | None -> raise (new System.MemberAccessException())        
    
        static member (?<-) (lookup:#DynamicLookup,name:string,value:'v) =
            lookup.SetValue (name,value)

        static member GetValue (lookup:DynamicLookup,name) =
            lookup.GetValue(name).Value