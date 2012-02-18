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

namespace Castle.MonoRail.Routing

open System
open System.Text
open System.Globalization
open System.Collections.Generic
open Microsoft.FSharp.Text.Lexing
open SimpleTokensLex
open System.Runtime.Serialization


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

    let rec RecursiveGenerate (buffer:StringBuilder) (nodeIndex:int) (nodes:list<Term>) (pending:List<string>) (namedParams:IDictionary<string,string>) (defValues:IDictionary<string,string>) = 
        if (nodeIndex > nodes.Length - 1) then
            true, ""
        else
            let node = nodes.[nodeIndex]

            match node with 
                | Literal (lit) -> 
                    
                    for s in pending do
                        buffer.Append s |> ignore
                    pending.Clear()

                    buffer.Append (lit) |> ignore
                    RecursiveGenerate buffer (nodeIndex+1) nodes pending namedParams defValues

                | NamedParam (lit,name) -> 

                    let hasParam, paramVal = namedParams.TryGetValue name
                    let hasDefVal, defVal = defValues.TryGetValue name

                    if not hasParam then 
                        false, (sprintf "Missing required parameter for route generation: '%s'" name)
                    else 
                        let value = lit + paramVal

                        if hasDefVal && (String.Compare(paramVal, defVal, StringComparison.OrdinalIgnoreCase) = 0) then
                            pending.Add value
                        else
                            for s in pending do
                                buffer.Append s |> ignore
                            pending.Clear()
                            buffer.Append (value) |> ignore

                        RecursiveGenerate buffer (nodeIndex+1) nodes pending namedParams defValues

                | Optional (lst) -> 
                    // process children of optional node. since it's optional, we dont care for the result
                    // but we continue from where it last succeeded, so we use the returned index going fwd
                    let r, v = RecursiveGenerate buffer 0 lst pending namedParams defValues
                    RecursiveGenerate buffer (nodeIndex+1) nodes pending namedParams defValues


    let rec rec_fill_default_values index (nodes:list<Term>) (namedParams:Dictionary<string,string>) (defValues:IDictionary<string,string>) = 
        if (index < nodes.Length) then 
            let node = nodes.Item(index)
            match node with
            | NamedParam (lit,name) -> 
                let r, v = defValues.TryGetValue name
                if r then namedParams.[name] <- v
            | Optional (lst) ->
                rec_fill_default_values 0 lst namedParams defValues
            | _ -> ignore()
            
            rec_fill_default_values (index + 1) nodes namedParams defValues


    let rec RecursiveMatch (path:string) (pathIndex:int) (nodeIndex:int) (nodes:list<Term>) (namedParams:Dictionary<string,string>) (defValues:IDictionary<string,string>) hasChildren withinOptional = 
        // there's content to match                      but not enough nodes? 
        if (path.Length - pathIndex > 0) && (nodeIndex > nodes.Length - 1) then
            if hasChildren || withinOptional then
                true, pathIndex
            else 
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
                        let newindex = pathIndex + lit.Length
                        RecursiveMatch path newindex (nodeIndex + 1) nodes namedParams defValues hasChildren withinOptional

                | NamedParam (lit,name) -> 

                    let cmp = String.Compare(lit, 0, path, pathIndex, lit.Length, StringComparison.OrdinalIgnoreCase)
                    if (cmp <> 0) then
                        false, pathIndex
                    else 
                        let start = pathIndex + lit.Length

                        let mutable last = path.IndexOfAny([|'/';'.'|], start)
                        last <- (if last <> -1 then last else path.Length)

                        let value = path.Substring(start, last - start)
                        if (value.Length <> 0) then
                            namedParams.[name] <- value
                            RecursiveMatch path last (nodeIndex + 1) nodes namedParams defValues hasChildren withinOptional
                        
                        elif withinOptional then
                            let r, v = defValues.TryGetValue name
                            if r then 
                                namedParams.[name] <- v
                                RecursiveMatch path last (nodeIndex + 1) nodes namedParams defValues hasChildren withinOptional
                            else 
                                false, pathIndex

                        else
                            false, pathIndex

                | Optional (lst) -> 
                    // process children of optional node. since it's optional, we dont care for the result
                    // but we continue from where it last succeeded, so we use the returned index going fwd
                    let res, index = RecursiveMatch path pathIndex 0 lst namedParams defValues false true

                    if not res then
                        rec_fill_default_values 0 lst namedParams defValues
                        
                    // continue with other nodes
                    RecursiveMatch path index (nodeIndex + 1) nodes namedParams defValues hasChildren withinOptional


    let buildErrorMsgForToken(tokenStreamOut:token) : string = 
        let tokenStr = 
            match tokenStreamOut with 
            | SimpleTokensLex.CLOSE -> ")"
            | SimpleTokensLex.OPEN -> "("
            | SimpleTokensLex.COLON -> ":"
            | SimpleTokensLex.DOT -> "."
            | SimpleTokensLex.SLASH -> "/"
            | _ -> "undefined";
        ExceptionBuilder.UnexpectedToken (tokenStr)

    let buildErrorMsg(tokenStreamOut:Option<token * TokenStream>) : string = 
        match tokenStreamOut with 
        | Some(t, tmp) as token -> 
            let first, s = token.Value
            buildErrorMsgForToken(first)
        | _ -> ExceptionBuilder.UnexpectedEndTokenStream

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

