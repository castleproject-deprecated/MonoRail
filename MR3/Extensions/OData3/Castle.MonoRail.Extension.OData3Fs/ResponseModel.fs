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


    type ODataRequestMessage(request:System.Web.HttpRequestBase, headers, contentType) = 
        let mutable _contentType : string = contentType
        let mutable _headers : IEnumerable<KeyValuePair<string, string>> = headers
        let mutable _method = request.HttpMethod
        let mutable _url = request.Url

        member x.ContentType with get() = _contentType and set(v) = _contentType <- v
        member x.Url with get() = _url and set(v) = _url <- v
        member x.GetHeader (headerName:string) = 
            request.Headers.Get(headerName)

        interface IODataRequestMessage with
            member x.Url with get() = _url and set(v) = _url <- v
            member x.Method with get() = _method and set(v) = _method <- v
            member x.GetStream() = request.InputStream
            member x.Headers = _headers
            member x.GetHeader(headerName) = 
                x.GetHeader(headerName)
            member x.SetHeader(headerName, value) = 
                raise(NotImplementedException())

        interface IODataUrlResolver with 
            member x.ResolveUrl(baseUri, payloadUri) = 
                payloadUri


    type ODataResponseMessage(response:System.Web.HttpResponseBase) = 
        let mutable _contentType : string = null
        let mutable _statusCode = 200
        let mutable _statusDesc = "OK"
        let mutable _headers : IEnumerable<KeyValuePair<string, string>> = Seq.empty

        member x.Status = _statusCode

        member x.Clear() = 
            response.Clear()

        member x.ContentType with get() = _contentType and set(v) = _contentType <- v
        member x.SetStatus(code, desc) = 
            _statusCode <- code
            _statusDesc <- desc

        member x.SetHeader(headerName, value) = 
            match headerName with 
            | "Content-Type" ->
                response.ContentEncoding <- System.Text.Encoding.UTF8
                response.ContentType <- value
            | "DataServiceVersion" ->
                response.AddHeader ("DataServiceVersion", value)
            | _ -> failwithf "Unsupported header attempt to be set %s" headerName

        interface IODataResponseMessage with 
            member x.Headers = _headers
            member x.StatusCode with get () = _statusCode and set(v) = _statusCode <- v
            member x.GetHeader(headerName) = _contentType
            member x.SetHeader(headerName,value) = 
                x.SetHeader(headerName,value)
            member x.GetStream() = response.OutputStream


    type PayloadKind = 
        | Unset = 0 
        | Feed  = 1
        | Entry = 2
        | Property = 3
        | Value = 4
        | Collection = 5
        | Error = 6



    type ResponseToSend = {
        Kind : PayloadKind 
        mutable QItems : IQueryable
        mutable SingleResult : obj
        FinalResourceUri : Uri
        EdmEntSet : IEdmEntitySet
        EdmType : IEdmType
        EdmProperty : IEdmProperty
        PropertiesToExpand : HashSet<IEdmProperty>
    }


    [<AutoOpen>]
    module ResponseModel = 
        
        let internal emptyResponse = { 
            Kind = PayloadKind.Unset
            EdmEntSet = null
            EdmType = null
            EdmProperty = null
            QItems = null; SingleResult = null; 
            FinalResourceUri=null; PropertiesToExpand = HashSet() 
        }



