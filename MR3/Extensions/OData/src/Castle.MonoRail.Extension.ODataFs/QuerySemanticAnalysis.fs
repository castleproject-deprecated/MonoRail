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
open System.Collections
open System.Collections.Specialized
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open System.Linq
open System.Linq.Expressions
open System.Text
open System.Reflection
open System.Web
open Castle.MonoRail
open FParsec
open FParsec.Primitives
open FParsec.CharParsers


type QueryAst = 
    | Element
    | Null
    | Literal of Type * obj
    | PropertyAccess of QueryAst * PropertyInfo * ResourceType
    | BinaryExp of QueryAst * QueryAst * BinaryOp * ResourceType
    | UnaryExp of QueryAst * UnaryOp * ResourceType
    with 
        member x.ToStringTree() = 
            let b = StringBuilder()
            let rec print (n) (level:int) = 
                b.AppendLine() |> ignore
                for x in 1..level do b.Append("  ") |> ignore
                match n with 
                | Null                      -> b.Append (sprintf "Null") |> ignore
                | Element                   -> b.Append (sprintf "Element") |> ignore
                | Literal (t,v)             -> b.Append (sprintf "Literal %s [%O]" t.Name v) |> ignore
                | PropertyAccess (ex,pinfo,rt) -> 
                    b.Append (sprintf "PropertyAccess [%s] = %s" pinfo.Name rt.FullName) |> ignore
                    print ex (level + 1)
                
                | UnaryExp (ex,op,rt)       -> 
                    b.Append (sprintf "Unary %O %s" op rt.FullName) |> ignore
                    print ex (level + 1)
                
                | BinaryExp (ex1,ex2,op,rt) -> 
                    b.Append (sprintf "Binary %O %s" op rt.FullName) |> ignore
                    print ex1 (level + 1)
                    print ex2 (level + 1)
                
                | _ -> failwithf "Unsupported node type? Need to update this match, dude"
            print x 1
            b.ToString()

module QuerySemanticAnalysis =
    begin
        
        let analyze_and_convert (exp:Exp) (rt:ResourceType) : QueryAst = 
            
            let rec r_analyze e (rt:ResourceType) = 
                match e with 
                | Exp.Element          -> QueryAst.Element, rt

                | Exp.Literal (edm, v) ->
                    let literal = 
                        match edm with
                        | EdmPrimitives.Null      -> QueryAst.Null 
                        | EdmPrimitives.SString   -> QueryAst.Literal (typeof<string>, v)
                        | EdmPrimitives.Int16     -> QueryAst.Literal (typeof<int16>, Convert.ToInt16(v))
                        | EdmPrimitives.Int32     -> QueryAst.Literal (typeof<int32>, Convert.ToInt32(v))
                        | EdmPrimitives.Int64     -> QueryAst.Literal (typeof<int64>, Convert.ToInt64(v))
                        | EdmPrimitives.Single    -> QueryAst.Literal (typeof<float32>, Convert.ToSingle(v))
                        | EdmPrimitives.Decimal   -> QueryAst.Literal (typeof<decimal>, Convert.ToDecimal(v))
                        | EdmPrimitives.Double    -> QueryAst.Literal (typeof<double>, Convert.ToDouble(v))
                        | EdmPrimitives.DateTime  -> QueryAst.Literal (typeof<DateTime>, DateTime.Parse(v))
                        | EdmPrimitives.Boolean   -> QueryAst.Literal (typeof<bool>, Convert.ToBoolean(v))
                        | EdmPrimitives.Guid      -> QueryAst.Literal (typeof<Guid>, Guid.Parse(v))
                        | _ -> failwithf "Unsupported edm primitive type %O" edm
                    
                    match literal with 
                    | QueryAst.Literal (t,v) -> literal, ResourceType.GetPrimitiveResourceType(t)
                    | QueryAst.Null          -> literal, null
                    | _ -> failwith "What kind of literal is that?!"

                | Exp.MemberAccess (ex, id) ->
                    let name = 
                        match id with 
                        | Identifier i -> i
                        | _ -> failwith "Only Identifier nodes are supported as the rhs of a MemberAccess node"

                    let get_prop (name:string) (rt:ResourceType) = 
                        match rt.Properties |> Seq.tryFind (fun p -> p.Name === name) with
                        | Some p -> p
                        | _ -> failwith "Property not found?"

                    let root, nestedRt = r_analyze ex rt

                    // rt.InstanceType.GetProperty(p.Name, BindingFlags.Public ||| BindingFlags.Instance)
                    let prop = get_prop name nestedRt
                    let propInfo = nestedRt.InstanceType.GetProperty(prop.Name, BindingFlags.Public ||| BindingFlags.Instance)

                    QueryAst.PropertyAccess(root, propInfo, prop.ResourceType), prop.ResourceType


                | Exp.Binary (ex1, op, ex2) ->
                    
                    let texp1, t1 = r_analyze ex1 rt
                    let texp2, t2 = r_analyze ex2 rt

                    let newExp1, newExp2, eqRt = 
                        match op with 
                        | BinaryOp.Add
                        | BinaryOp.Mul
                        | BinaryOp.Div
                        | BinaryOp.Mod
                        | BinaryOp.Sub -> 
                            // suports decimal, double single int32 and int64
                            // need to promote members if necessary
                            texp1, texp2, t1 (* temporary! *)

                        | BinaryOp.And
                        | BinaryOp.Or  ->
                            // suports booleans
                            texp1, texp2, ResourceType.GetPrimitiveResourceType(typeof<bool>)

                        | BinaryOp.Neq
                        | BinaryOp.Eq 
                        | BinaryOp.LessT 
                        | BinaryOp.GreatT
                        | BinaryOp.LessET
                        | BinaryOp.GreatET  -> 
                            // suports double single int32 int64 string datetime guid binary
                            texp1, texp2, ResourceType.GetPrimitiveResourceType(typeof<bool>)


                        | _ -> failwith "Unknown binary operation"

                    QueryAst.BinaryExp(newExp1, newExp2, op, eqRt), eqRt


                | Exp.Unary (op, exp) ->

                    let exp1, expRt = r_analyze exp rt

                    let newExp, eqRt = 
                        match op with 
                        | UnaryOp.Negate ->
                            // suports decimal, double single int32 and int64
                            // need to promote members if necessary
                            exp1, expRt

                        | UnaryOp.Not -> 
                            // suports bool only
                            exp1, ResourceType.GetPrimitiveResourceType(typeof<bool>)

                        // TODO: isofExpression
                        // TODO: cast
                        | _ -> failwith "Unknown unary operation"

                    QueryAst.UnaryExp(newExp, op, eqRt), eqRt

                | _ -> failwithf "Unsupported exp type %O" e

            let newTree, _ = r_analyze exp rt
            newTree

    end

