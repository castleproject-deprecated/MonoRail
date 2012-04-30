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
    open System.Collections.Generic
    open System.Web
    open System.Runtime.InteropServices
    open Castle.MonoRail.Routing
    open System.Text
    open System.Globalization
    open System.Collections.Generic
    open System.Runtime.Serialization
    open FParsec


    type ResourceLink(uri:string, rel:string, contenttype:string, label:string) = 
        let mutable _uri = uri
        let mutable _rel = rel
        let mutable _label = label
        let mutable _contenttype = contenttype

        new() = 
            ResourceLink(null, null, null, null)

        member x.Uri 
            with get() = _uri and set(v) = _uri <- v
        member x.Label 
            with get() = _label and set(v) = _label <- v
        member x.Rel
            with get() = _rel and set(v) = _rel <- v
        member x.ContentType 
            with get() = _contenttype and set(v) = _contenttype <- v


    [<Serializable>]
    type Resource<'a>(resource:'a) = 
        let _links = lazy List<ResourceLink>()
        member x.Links = _links.Force()
        member x.Resource = resource


    [<Interface>]
    type IModelHypertextProcessor<'a> = 
        abstract member AddHypertext : model:'a -> unit
        

    [<System.ComponentModel.Composition.Export;AllowNullLiteral>]
    type ModelHypertextProcessorResolver() = 
        let _dict = lazy Dictionary<Type, obj>()

        member x.Register<'TModel> (processor:IModelHypertextProcessor<'TModel>) = 
            _dict.Force().[typeof<'TModel>] <- box processor

        member x.TryGetProcessor<'a>([<Out>] value:IModelHypertextProcessor<'a> byref) =
            if _dict.IsValueCreated then
                let res, boxedVal = _dict.Value.TryGetValue typeof<'a>
                if res then
                    value <- boxedVal :?> IModelHypertextProcessor<'a>
                    true
                else
                    false
            else 
                false

    module AcceptHeaderParser = 
        begin
            
            type AcceptVal = {
                media : string;
                sub : string;
                quality : decimal;
                level : int;
            }

            let pval = manyChars (noneOf ";") 
            let pid = manyChars (noneOf "=;/") 
            let quality = pstringCI "q=" >>. pval |>> fun v -> ("Q", v)
            let arb = pstringCI "level=" >>. pval |>> fun v -> ("L", v)
            let token = choice [ quality; arb ] 
            let media = pid .>> pchar '/' .>>. pid |>> fun m -> (fst m, snd m)

            let term = 
                // text/html;q=0.7;level=1
                // text/*;q=0.7;level=1
                // */*;q=0.7;level=1
                media .>>. sepBy token (pchar ';') 
                |>> fun ((m,s),l) -> ( 
                        let q, tokens = 
                            let qs,ts = l |> List.partition (fun (h,q) -> h = "Q")
                            let ts = ts |> List.map (fun (h,q) -> Int32.Parse(q))
                            if List.isEmpty qs 
                            then 1.0m, ts 
                            else Decimal.Parse (snd <| List.head qs), ts
                        { media = m; sub = s; quality = q; level = 1 } 
                    )

            let parse (accept:string []) = 
        (*
                    Accept: text/*, text/html, text/html;level=1, * / *

                    have the following precedence:

                    1) text/html;level=1
                    2) text/html
                    3) text/*
                    4) */*

                    Accept: 
                            text/*;q=0.3, 
                            text/html;q=0.7, 
                            text/html;level=1,
                            text/html;level=2;q=0.4, 
                            */*;q=0.5

                    would cause the following values to be associated:

                    text/html;level=1         = 1
                    text/html                 = 0.7
                    text/plain                = 0.3

                    image/jpeg                = 0.5
                    text/html;level=2         = 0.4
                    text/html;level=3         = 0.7
        *)
                let sort (l:AcceptVal) (r:AcceptVal) = 
                    0

                let values = 
                    accept |> Array.map (fun ac -> 
                                match run term ac with 
                                | Success(result, _, _) -> result 
                                | Failure(errorMsg, _, _) -> (raise(ArgumentException(errorMsg))))
                
                values |> Array.sortInPlaceWith sort

        end

    //
    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
    //
    [<System.ComponentModel.Composition.Export;AllowNullLiteral>]
    type ContentNegotiator() = 

        let acceptheader_to_mediatype (acceptHeader:string []) = 
            
            if acceptHeader = null || acceptHeader.Length = 0 then
                MediaTypes.Html
            else
                let app, text  = 
                    acceptHeader
                    |> Seq.map (fun (h:string) -> let parts = h.Split([|'/';';'|])
                                                  (parts.[0], parts.[1]) )
                    |> Seq.toList
                    |> List.partition (fun (t1:string,t2:string) -> t1 = "application")

                if not (List.isEmpty app) then
                    let _, firstapp = app.Head 
                    match firstapp with 
                    | "json" -> MediaTypes.JSon
                    | "xml" -> MediaTypes.Xml
                    | "atom+xml" -> MediaTypes.Atom
                    | "rss+xml" -> MediaTypes.Rss
                    | "javascript" | "js" -> MediaTypes.Js
                    | "soap+xml" -> MediaTypes.Soap
                    | "xhtml+xml" -> MediaTypes.XHtml
                    | "x-www-form-urlencoded" -> MediaTypes.FormUrlEncoded
                    // | "soap+xml" -> Js
                    | _ -> null
                elif not (List.isEmpty text) then
                    match text.Head with 
                    | _,"xml" -> MediaTypes.Xml
                    | _,"html" -> MediaTypes.Html
                    | _,"javascript" -> MediaTypes.Js
                    | "multipart","form-data" -> MediaTypes.FormUrlEncoded // http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
                    | _ -> null
                    // csv
                else MediaTypes.Html

        member x.ResolveBestContentType (accept:string[], supports:string seq) = 
            if accept = null || accept.Length = 0 then Seq.head supports
            else 
                // text/html;q=0.7;level=1
                AcceptHeaderParser.parse accept 
                ""

        member x.ResolveContentType (contentType:string) = 
            if String.IsNullOrEmpty contentType then raise (ArgumentNullException("contentType"))
            let media = acceptheader_to_mediatype [|contentType|] 
            if media = null 
            then contentType // possibly a custom format
            else media


        member x.ResolveRequestedMediaType (route:RouteMatch) (request:HttpRequestBase) = 
            let r, format = route.RouteParams.TryGetValue "format"
            if r then 
                // todo: this should delegate to a service provided by the app, so it can be extended. 
                match format with
                | "html" -> MediaTypes.Html
                | "json" -> MediaTypes.JSon
                | "rss"  -> MediaTypes.Rss
                | "js"   -> MediaTypes.Js
                | "atom" -> MediaTypes.Atom
                | "xml"  -> MediaTypes.Xml
                | _ -> failwithf "Unknown format %s" format
            else
                let accept_header = request.AcceptTypes
                let media = acceptheader_to_mediatype accept_header 
                if media = null 
                then failwith "Unknown format in accept header"  
                else media

