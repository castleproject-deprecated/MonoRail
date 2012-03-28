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

[<AutoOpen>]
module RefHelpers
    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Linq.Expressions

    let internal guard_load_types (asm:Assembly) =
        try
            asm.GetTypes()
        with
        | :? ReflectionTypeLoadException as exn -> 
            exn.Types

    let internal guard_load_public_types (asm:Assembly) =
        try
            asm.GetExportedTypes()
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

    let read_att_filter<'a when 'a : null> (prov:#ICustomAttributeProvider) (filter:'a -> bool) : 'a = 
        let attrs = prov.GetCustomAttributes(typeof<'a>, true)
        if (attrs.Length = 0) then
            null
        else 
            let filtered = attrs |> Array.toSeq |> Seq.cast<'a> |> Seq.filter filter
            if Seq.length filtered = 1 then
                Seq.head filtered
            else
                failwithf "Expected a single %s, but found many in provider %O" (typeof<'a>.Name) (prov.GetType())

    let read_att<'a> (prov:#ICustomAttributeProvider) : 'a = 
        let attrs = prov.GetCustomAttributes(typeof<'a>, true)
        if (attrs.Length = 0) then
            Unchecked.defaultof<'a>
        elif (attrs.Length = 1) then
            attrs.[0] :?> 'a
        else
            // failwithf "Expected a single, but found many in provider %O" (prov.GetType())
            failwithf "Expected a single %s, but found many in provider %O" (typeof<'a>.Name) (prov.GetType())

    (*
    let read_att2<'a when 'a : null> (prov:#ICustomAttributeProvider) (proc:'a -> 'b) (def:'b) : 'b = 
        let attrs = prov.GetCustomAttributes(typeof<'a>, true)
        if (attrs.Length = 0) then
            def
        elif (attrs.Length = 1) then
            proc(attrs.[0] :?> 'a)
        else
            // failwithf "Expected a single, but found many in provider %O" (prov.GetType())
            failwithf "Expected a single %s, but found many in provider %O" (typeof<'a>.Name) (prov.GetType())
    *)      

    let propinfo_from_exp (exp:Expression<Func<'a, obj>>) : PropertyInfo array = 
        if exp.NodeType <> ExpressionType.Lambda then
            raise (ArgumentException ("Expression should be a lambda", "propertyAccess"))
        else
            let mutable curExp : Expression = exp.Body
            let propList = List()
            while curExp.NodeType <> ExpressionType.Parameter do

                if curExp.NodeType = ExpressionType.MemberAccess then
                    let memberAccess = curExp :?> MemberExpression
                    propList.Add (memberAccess.Member :?> PropertyInfo)
                    curExp <- memberAccess.Expression
                else 
                    failwithf "Unexpected node type %O " (curExp.NodeType)

            propList.Reverse() 
            propList.ToArray()

    let rec method_from_exp_r (exp:Expression) = 
        match exp.NodeType with
        | ExpressionType.MemberAccess -> 
            let memberAccess = exp :?> MemberExpression
            match memberAccess.Member with
            | :? MethodInfo as mi -> mi
            | _ -> null
        | ExpressionType.Parameter -> null
        | _ -> null

    and method_from_exp (exp:Expression<Func<'a, obj>>) : MethodInfo = 
        if exp.NodeType <> ExpressionType.Lambda then
            raise (ArgumentException ("Expression should be a lambda", "methodAccess"))
        method_from_exp_r( exp.Body)




