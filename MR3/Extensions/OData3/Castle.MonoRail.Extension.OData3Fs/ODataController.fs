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
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Text
    open System.Reflection
    open System.Web
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.OData
    open Castle.MonoRail.Routing
    open Castle.MonoRail.OData.Internal
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library


    /// Entry point for exposing EntitySets through OData
    [<AbstractClass>]
    type ODataController<'T when 'T : not struct and 'T :> ODataModel>(modelTemplate:'T) =  

        // should be cached using the subclass as key

        let _modelToUse = ref modelTemplate

        let _services : Ref<IServiceRegistry> = ref null

        let resolveHttpOperation (httpMethod) = 
            match httpMethod with 
            | SegmentProcessor.HttpGet    -> RequestOperation.Get
            | SegmentProcessor.HttpPost   -> RequestOperation.Create
            | SegmentProcessor.HttpPut    -> RequestOperation.Update
            | SegmentProcessor.HttpDelete -> RequestOperation.Delete
            | SegmentProcessor.HttpMerge  -> RequestOperation.Merge
            | _ -> failwithf "Unsupported http method %s" httpMethod


        member x.Model = !_modelToUse

        member x.Process(services:IServiceRegistry, httpMethod:string, greedyMatch:string, 
                         routeMatch:RouteMatch, context:HttpContextBase) = 

            _services := services

            let odataModel = modelTemplate
            if not odataModel.IsInitialized then
                lock(odataModel) 
                    (fun _ -> if not odataModel.IsInitialized then odataModel.InitializeModels(services))

            let edmModel = odataModel.EdmModel
            let request  = context.Request
            let response = context.Response
            let writer   = response.Output

            let qs = request.Url.Query
            let baseUri = routeMatch.Uri
            let requestContentType, reqEncoding = 
                if request.ContentType.IndexOf(";", StringComparison.Ordinal) <> -1 then 
                    let content = request.ContentType.Split([|';'|]).[0]
                    content, request.ContentEncoding
                else request.ContentType, request.ContentEncoding

            let negotiate_content (isSingle) = 
                let supported = 
                    if isSingle 
                    then [|"application/atom+xml";"application/json";"application/xml";"text/plain"|]
                    else [|"application/atom+xml";"application/json"|]
                services.ContentNegotiator.ResolveBestContentType (request.AcceptTypes, supported)

            let invoke action isColl (rt:IEdmType) (parameters:(Type*obj) seq) value isOptional : bool * obj = 
                let ctxCreation = Func<ControllerCreationContext>(fun c -> ControllerCreationContext(routeMatch, context))
                odataModel.InvokeSubController(ctxCreation, action, isColl, rt, parameters, value, isOptional)

            let callbacks = {
                intercept     = Func<IEdmType,(Type*obj) seq,obj,obj> (fun rt ps o         -> invoke "Intercept"     false rt ps o true |> snd);  
                interceptMany = Func<IEdmType,(Type*obj) seq,IEnumerable,IEnumerable> (fun rt ps o -> invoke "InterceptMany" false rt ps o true |> snd :?> IEnumerable);  
                authorize     = Func<IEdmType,(Type*obj) seq,obj,bool>(fun rt ps o         -> invoke "Authorize"     false rt ps o true |> fst);  
                authorizeMany = Func<IEdmType,(Type*obj) seq,IEnumerable,bool>(fun rt ps o -> invoke "AuthorizeMany" true  rt ps o true |> fst);  
                view          = Func<IEdmType,(Type*obj) seq,obj,bool>(fun rt ps o         -> invoke "View"          false rt ps o true |> fst);  
                viewMany      = Func<IEdmType,(Type*obj) seq,IEnumerable,bool>(fun rt ps o -> invoke "ViewMany"      true  rt ps o true |> fst);  
                create        = Func<IEdmType,(Type*obj) seq,obj,bool>(fun rt ps o         -> invoke "Create"        false rt ps o false |> fst);  
                update        = Func<IEdmType,(Type*obj) seq,obj,bool>(fun rt ps o         -> invoke "Update"        false rt ps o false |> fst);  
                remove        = Func<IEdmType,(Type*obj) seq,obj,bool>(fun rt ps o         -> invoke "Remove"        false rt ps o false |> fst);  
                operation     = Func<IEdmType,(Type*obj) seq,string,obj>(fun rt ps action  -> invoke action          false rt ps null false |> snd);
                negotiateContent = Func<bool, string>(negotiate_content)
            }

            let requestHeaders = 
                request.Headers.Keys 
                |> Seq.cast<string> 
                |> Seq.map ( fun k -> KeyValuePair(k, request.Headers.[k]) )
                |> Seq.toArray

            let requestMessage  = ODataRequestMessage(request, requestHeaders, requestContentType)
            let responseMessage = ODataResponseMessage(response)
            let serializer      = ODataStackPayloadSerializer(edmModel, request.Url)

            try
                try
                    let op = resolveHttpOperation httpMethod
                    let segments, meta, metaquery = SegmentParser.parse (greedyMatch, request.QueryString, edmModel, baseUri)
 
                    SegmentProcessor.Process edmModel odataModel op 
                                             segments meta metaquery 
                                             request.QueryString 
                                             callbacks 
                                             requestMessage responseMessage serializer

                    (*
                    if responseParams.httpStatus <> 200 then
                        response.StatusCode <- responseParams.httpStatus
                        response.StatusDescription <- responseParams.httpStatusDesc
                    if not <| String.IsNullOrEmpty responseParams.contentType then
                        response.ContentType <- responseParams.contentType
                    if responseParams.contentEncoding <> null then 
                        response.ContentEncoding <- responseParams.contentEncoding
                    if responseParams.location <> null then
                        response.AddHeader("Location", responseParams.location)
                    *)

                    EmptyResult.Instance
                with 
                | exc -> reraise()

            finally 
                ()
                // cleanup



