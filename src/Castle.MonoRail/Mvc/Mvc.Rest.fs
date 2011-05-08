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

        new() = ResourceLink(null, null, null, null)

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

        member x.AddProcessor<'a> (processor:IModelHypertextProcessor<'a>) = 
            _dict.Force().[typeof<'a>] <- box processor

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

        let (|Xhtml|Json|Js|Atom|Xml|Rss|Unknown|) (acceptHeader:string []) = 
            if (acceptHeader == null || acceptHeader.Length = 0) then
                Xhtml
            else
                let app, text  = 
                    acceptHeader
                    |> Seq.map (fun (h:string) -> (
                                                    let parts = h.Split([|'/';';'|])
                                                    (parts.[0], parts.[1])
                                                   )  )
                    |> Seq.toList
                    |> List.partition (fun (t1:string,t2:string) -> t1 = "application")

                if not (List.isEmpty app) then
                    let tmp, firstapp = app.Head 
                    match firstapp with 
                    | "json" -> Json
                    | "atom+xml" -> Atom
                    | "rss+xml" -> Rss
                    | "javascript" | "js" -> Js
                    | "soap+xml" -> Js
                    | "xhtml+xml" | "xml" -> Xhtml
                    // | "soap+xml" -> Js
                    | _ -> Unknown
                elif not (List.isEmpty text) then
                    let tmp, firsttxt = text.Head 
                    match firsttxt with 
                    | "xml" -> Xml
                    | "html" -> Xhtml
                    | "javascript" -> Js
                    | _ -> Unknown
                    // csv
                else 
                    Xhtml

        member x.ResolveMimeTypeForRequest (route:RouteMatch) (request:HttpRequestBase) = 
            let r, format = route.RouteParams.TryGetValue "format"
            if r then 
                match format with
                | "html" -> MimeType.Xhtml
                | "json" -> MimeType.JSon
                | "rss" -> MimeType.Rss
                | "js" -> MimeType.Js
                | "atom" -> MimeType.Atom
                | "xml" -> MimeType.Xml
                | _ -> failwithf "Unknown format %s " format
            else 
                let accept_header = request.AcceptTypes
                match accept_header with
                | Xhtml -> MimeType.Xhtml
                | Json -> MimeType.JSon
                | Rss -> MimeType.Rss
                | Js -> MimeType.Js
                | Atom -> MimeType.Atom
                | Xml -> MimeType.Xml
                | Unknown | _ -> failwith "Unknown format in accept header"  
        