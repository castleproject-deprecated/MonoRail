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

namespace Castle.MonoRail.OData.Internal

    open System
    open System.IO
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library


    type RequestOperation = 
        | Get = 0 
        | Create = 1
        | Update = 3
        | Delete = 4
        | Merge = 5


    type ODataRequestMessage(stream, ``method``, url, contentType, headers) = 
        let mutable _contentType : string = contentType
        let mutable _headers : IEnumerable<KeyValuePair<string, string>> = headers
        let mutable _method = ``method``
        let mutable _url = url

        member x.ContentType with get() = _contentType and set(v) = _contentType <- v

        interface IODataRequestMessage with
            member x.Url with get() = _url and set(v) = _url <- v
            member x.Method with get() = _method and set(v) = _method <- v
            member x.GetStream() = stream
            member x.Headers = _headers
            member x.GetHeader(headerName) = 
                raise(NotImplementedException())
            member x.SetHeader(headerName, value) = 
                raise(NotImplementedException())

        interface IODataUrlResolver with 
            member x.ResolveUrl(baseUri, payloadUri) = 
                payloadUri


    type ODataResponseMessage() = 
        let mutable _contentType : string = null
        let mutable _statusCode : int = 200
        let mutable _headers : IEnumerable<KeyValuePair<string, string>> = Seq.empty
        let mutable _stream : Stream = null

        interface IODataResponseMessage with 
            member x.Headers = _headers
            member x.StatusCode with get () = _statusCode and set(v) = _statusCode <- v
            member x.GetHeader(headerName) = _contentType
            member x.SetHeader(headerName, value) = 
                match headerName with 
                | "Content-Type" ->
                    ()
                | "DataServiceVersion" ->
                    ()
                | _ -> failwithf "Unsupported header attempt to be set %s" headerName

            member x.GetStream() = _stream
                

    (*
    type RequestParameters = {
        model : ODataModel
        contentType: string
        contentEncoding : Encoding
        input: Stream
        baseUri : Uri
        accept: string[]
    }

    type ResponseParameters = {
        mutable contentType: string
        mutable contentEncoding : Encoding
        mutable httpStatus : int
        mutable httpStatusDesc : string
        mutable location : string
        writer : TextWriter
    }
    *)

    type ResponseToSend = {
        mutable QItems : IQueryable
        mutable SingleResult : obj
        // ResType : ResourceType
        FinalResourceUri : Uri
        // ResProp : ResourceProperty
        PropertiesToExpand : HashSet<IEdmProperty>
    }