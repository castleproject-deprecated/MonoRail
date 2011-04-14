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
//  This software is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Routing

open System
open System.Text
open System.Globalization
open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open SimpleTokensLex
open Option
open System.Runtime.Serialization
open ExceptionBuilder

[<Serializable>]
type RouteParsingException = 
    inherit Exception
    new (msg) = { inherit Exception(msg) }
    new (info:SerializationInfo, context:StreamingContext) = 
        { 
            inherit Exception(info, context)
        }


module Internal = 

    type Term = 
    | Literal of string
    | NamedParam of string * string
    | Optional of list<Term>

    type TokenStream = LazyList<token>


    let rec RecursiveMatch(path:string, pathIndex:int, nodeIndex:int, nodes:list<Term>, namedParams:IDictionary<string,string>) = 
        
        // there's content to match                      but not enough nodes? 
        if (path.Length - pathIndex > 0) && (nodeIndex > nodes.Length - 1) then
            false, pathIndex
        elif (nodeIndex > nodes.Length - 1) then
            true, pathIndex
        else
            let node = nodes.[nodeIndex]

            match node with 
                | Literal (lit) -> 

                    let cmp = String.Compare(lit, 0, path, pathIndex, lit.Length, StringComparison.OrdinalIgnoreCase)
                    if (cmp <> 0) then
                        false, pathIndex
                    else
                        RecursiveMatch(path, pathIndex + lit.Length, nodeIndex + 1, nodes, namedParams)

                | NamedParam (lit,name) -> 

                    let cmp = String.Compare(lit, 0, path, pathIndex, lit.Length, StringComparison.OrdinalIgnoreCase)
                    if (cmp <> 0) then
                        false, pathIndex
                    else 
                        let start = pathIndex + lit.Length

                        let mutable last = path.IndexOfAny([|'/';'.'|], start)
                        last <- (if last <> -1 then last else path.Length)

                        let value = path.Substring(start, last - start)
                        if (value <> String.Empty) then
                            namedParams.Add(name, value)

                        RecursiveMatch(path, last, nodeIndex + 1, nodes, namedParams)

                | Optional (lst) -> 
                    // process children of optional node. since it's optional, we dont care for the result
                    // but we continue from where it last succeeded, so we use the returned index going fwd
                    let res, index = RecursiveMatch(path, pathIndex, 0, lst, namedParams)
                    // continue with other nodes
                    RecursiveMatch(path, index, nodeIndex + 1, nodes, namedParams)


    let buildErrorMsgForToken(tokenStreamOut:token) : string = 
        let tokenStr = 
            match tokenStreamOut with 
            | SimpleTokensLex.CLOSE -> ")"
            | SimpleTokensLex.OPEN -> "("
            | SimpleTokensLex.COLON -> ":"
            | SimpleTokensLex.DOT -> "."
            | SimpleTokensLex.SLASH -> "/"
            | _ -> "undefined";
        UnexpectedToken (tokenStr)

    let buildErrorMsg(tokenStreamOut:Option<token * TokenStream>) : string = 
        match tokenStreamOut with 
        | Some(t, tmp) as token -> 
            let first, s = token.Value
            buildErrorMsgForToken(first)
        | _ -> UnexpectedEndTokenStream

    // Generate the token stream as a seq<token> 
    let CreateTokenStream  (inp : string) = 
        seq { let lexbuf = LexBuffer<_>.FromString inp 
              while not lexbuf.IsPastEndOfStream do 
                  match SimpleTokensLex.token lexbuf with 
                  | EOF -> yield! [] 
                  | token -> yield token } 
        |> LazyList.ofSeq
 

    let tryToken (src: TokenStream) = 
        match src with 
        | LazyList.Cons ((tok), rest) -> Some(tok, rest) 
        | _ -> None 


    let rec parseRoute (src) = 
        let t1, src = parseTerm src
        match tryToken src with
        | Some(CLOSE, temp) -> 
            [t1], src
        | Some(_, temp) -> 
            let p2, src = parseRoute src
            (t1 :: p2), src
        | _ as y -> 
            [t1], src


    and parseTerm(src) = 
        match tryToken src with
        | Some(DOT, temp) | Some(SLASH, temp) as t1 ->
            
            let tok, tmp = t1.Value
            let tokAsChar = 
                match tok with
                | DOT -> '.'
                | SLASH -> '/'
                | _ as errtoken -> 
                    raise (RouteParsingException(buildErrorMsgForToken(errtoken)))

            match tryToken temp with
            | Some(ID id, temp) -> NamedParam(tokAsChar.ToString(), id.Substring(1)), temp
            | Some(STRING lit, temp) -> Literal(String.Concat(tokAsChar.ToString(), lit)), temp
            | _ as errtoken -> 
                raise (RouteParsingException (buildErrorMsg(errtoken)))
        
        | Some(OPEN, temp) -> 
            
            let lst, src = parseRoute temp
            
            match tryToken src with 
            | Some(CLOSE, src) -> Optional(lst), src
            | _ as errtoken -> 
                raise (RouteParsingException (buildErrorMsg(errtoken)))

        | _ as errtoken -> 
            
            raise (RouteParsingException (buildErrorMsg(errtoken)))


    let parseRoutePath(path:string) = 
        let stream = CreateTokenStream (path)
        let list, temp = parseRoute(stream)
        list

