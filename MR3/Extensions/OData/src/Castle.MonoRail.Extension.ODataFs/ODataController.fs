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
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq
    open System.Text
    open System.Reflection
    open System.Web
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.OData
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extension.OData


    /// Entry point for exposing EntitySets through OData
    [<AbstractClass>]
    type ODataController<'T when 'T :> ODataModel>(model:'T) =  

        let _provider = model :> IDataServiceMetadataProvider
        let _wrapper  = DataServiceMetadataProviderWrapper(_provider)
        let _services : Ref<IServiceRegistry> = ref null

        let resolveHttpOperation (httpMethod) = 
            match httpMethod with 
            | SegmentProcessor.HttpGet    -> SegmentOp.View
            | SegmentProcessor.HttpPost   -> SegmentOp.Create
            | SegmentProcessor.HttpPut    -> SegmentOp.Update
            | SegmentProcessor.HttpDelete -> SegmentOp.Delete
            | _ -> failwithf "Unsupported http method %s" httpMethod

        let controller_cache = Dictionary<ResourceType, ControllerPrototype>()
        // let executor_cache   = Dictionary<ResourceType, ODataEntitySubControllerExecutor>()
        let invoker_cache   = Dictionary<ResourceType, string * obj seq -> obj * bool>()

        let get_action_invoker rt = 
            let create_controller_prototype (rt:ResourceType) = 
                let suc, value = controller_cache.TryGetValue rt
                if suc then value
                else 
                    let creator = model.GetControllerCreator (rt)
                    if creator <> null then
                        let prototype = creator.Invoke()
                        controller_cache.[rt] <- prototype
                        prototype
                    else null

            let create_executor_fn (rt:ResourceType) prototype = 
                let executor = (!_services).ControllerExecutorProvider.CreateExecutor(prototype)
                System.Diagnostics.Debug.Assert ( executor <> null && executor :? ODataEntitySubControllerExecutor )
                let odataExecutor = executor :?> ODataEntitySubControllerExecutor
                (fun (action,parameters) -> 
                    // odataExecutor.GetParameterCallback <- Func<Type,obj>(paramCallback)
                    executor.Execute(action, prototype, routeMatch, context) , true)
            
            let succ, existing = invoker_cache.TryGetValue rt
            if succ then existing 
            else
                let prototype = create_controller_prototype rt
                let executor = create_executor_fn rt prototype 
                invoker_cache.[rt] <- executor 
                executor

        let invoke_action rt action parameters = 
            // create prototype + executor
            // get_action_invoker rt 
            false

        let invoke_controller (action:string) isCollection (rt:ResourceType) parameters optional = 
            if model.SupportsAction(rt, action) then
                // let prototype = get_controller_prototype (rt)
                let result = invoke_action rt action parameters
                if result <> null && result :? EmptyResult 
                then true else false
            else if optional then true else false

        member x.Model = model
        member internal x.MetadataProvider = _provider
        member internal x.MetadataProviderWrapper = _wrapper

        member x.Process(services:IServiceRegistry, httpMethod:string, greedyMatch:string, 
                         routeMatch:RouteMatch, context:HttpContextBase) = 
            
            _services := services

            model.SetServiceRegistry services

            let request = context.Request
            let response = context.Response
            response.AddHeader("DataServiceVersion", "2.0")

            let writer = response.Output
            let qs = request.Url.Query
            let baseUri = routeMatch.Uri
            let requestContentType = request.ContentType

            let callbacks = {
                    authorize = Func<ResourceType,obj,bool>(fun rt o -> invoke_controller  "Authorize" false rt o true);  
                    authorizeMany = Func<ResourceType,IEnumerable,bool>(fun rt o -> true);  // Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Access" false rt o true);  
                    view = Func<ResourceType,obj,bool>(fun rt o -> true);  // Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Access" false rt o true);
                    viewMany = Func<ResourceType,IEnumerable,bool>(fun rt o -> true); // Func<ResourceType,IEnumerable,bool>(fun rt o -> invoke_controller "AccessMany" true rt o true);
                    create = Func<ResourceType,obj,bool>(fun rt o -> true); // Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Create" false rt o false);
                    update = Func<ResourceType,obj,bool>(fun rt o -> true); // Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Update" false rt o false);
                    remove = Func<ResourceType,obj,bool>(fun rt o -> true); // Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Remove" false rt o false);
                }
            let requestParams = { 
                    model = model; 
                    provider = x.MetadataProvider; 
                    wrapper = x.MetadataProviderWrapper; 
                    contentType = requestContentType; 
                    contentEncoding = request.ContentEncoding;
                    input = request.InputStream; 
                    baseUri = baseUri; 
                    accept = request.AcceptTypes;
                }
            let responseParams = { 
                    contentType = null ;
                    contentEncoding = response.ContentEncoding;
                    writer = writer;
                    httpStatus = 200;
                    httpStatusDesc = "OK";
                    location = null;
                }

            try
                let op = resolveHttpOperation httpMethod
                let segments = SegmentParser.parse (greedyMatch, qs, model, baseUri)

                SegmentProcessor.Process op segments callbacks requestParams responseParams

                if responseParams.httpStatus <> 200 then
                    response.StatusCode <- responseParams.httpStatus
                    response.StatusDescription <- responseParams.httpStatusDesc
                if not <| String.IsNullOrEmpty responseParams.contentType then
                    response.ContentType <- responseParams.contentType
                if responseParams.contentEncoding <> null then 
                    response.ContentEncoding <- responseParams.contentEncoding
                if responseParams.location <> null then
                    response.AddHeader("Location", responseParams.location)

                EmptyResult.Instance

            with 
            | :? HttpException as ht -> reraise()
            | exc -> 
                // todo: instead of raising, we should serialize error e write it back

                // TODO: use responseContentType to resolve output mime type

                // in json: 
                (* 
                    { "error": {
                        "code": "", 
                        "message": {
                            "lang": "en-US", 
                            "value": "Resource not found for the segment 'People'."
                        } } 
                    }
                *)

                // in xml: 
                (* 
                    <?xml version="1.0" encoding="utf-8" standalone="yes"?>
                    <error xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
                      <code></code>
                      <message xml:lang="en-US">Resource not found for the segment 'People'.</message>
                    </error>                
                *)

                // if html, let the exception filters handle it, otherwise, let it bubble to asp.net

                reraise()


