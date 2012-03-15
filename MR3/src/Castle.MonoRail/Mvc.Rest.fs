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
        

    [<System.ComponentModel.Composition.Export()>]
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


    [<System.ComponentModel.Composition.Export()>]
    type ContentNegotiator() = 

        let header_to_mime (acceptHeader:string []) = 
            if acceptHeader = null || acceptHeader.Length = 0 then
                MimeType.Html
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
                    | "json" -> MimeType.JSon
                    | "xml" -> MimeType.Xml
                    | "atom+xml" -> MimeType.Atom
                    | "rss+xml" -> MimeType.Rss
                    | "javascript" | "js" -> MimeType.Js
                    // | "soap+xml" -> MimeType.Js
                    | "xhtml+xml" -> MimeType.Html
                    | "x-www-form-urlencoded" -> MimeType.FormUrlEncoded
                    // | "soap+xml" -> Js
                    | _ -> MimeType.Unknown
                elif not (List.isEmpty text) then
                    // let tmp, firsttxt = text.Head 
                    match text.Head with 
                    | _,"xml" -> MimeType.Xml
                    | _,"html" -> MimeType.Html
                    | _,"javascript" -> MimeType.Js
                    | "multipart","form-data" -> MimeType.FormUrlEncoded // http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
                    | _ -> MimeType.Unknown
                    // csv
                else 
                    MimeType.Html

        member x.ResolveContentType (contentType:string) = 
            if String.IsNullOrEmpty contentType then raise (ArgumentNullException("contentType"))
            match header_to_mime [|contentType|] with
            | MimeType.Unknown -> failwith "Unknown format in content-type"  
            | _ as mime -> mime

        member x.ResolveRequestedMimeType (route:RouteMatch) (request:HttpRequestBase) = 
            let r, format = route.RouteParams.TryGetValue "format"
            if r then 
                match format with
                | "html" -> MimeType.Html
                | "json" -> MimeType.JSon
                | "rss" -> MimeType.Rss
                | "js" -> MimeType.Js
                | "atom" -> MimeType.Atom
                | "xml" -> MimeType.Xml
                | _ -> failwithf "Unknown format %s " format
            else 
                let accept_header = request.AcceptTypes
                match header_to_mime accept_header with
                | MimeType.Unknown -> failwith "Unknown format in accept header"  
                | _ as mime -> mime


        