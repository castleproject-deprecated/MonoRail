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

        let resolveHttpOperation (httpMethod) = 
            match httpMethod with 
            | SegmentProcessor.HttpGet    -> SegmentOp.View
            | SegmentProcessor.HttpPost   -> SegmentOp.Create
            | SegmentProcessor.HttpPut    -> SegmentOp.Update
            | SegmentProcessor.HttpDelete -> SegmentOp.Delete
            | _ -> failwithf "Unsupported http method %s" httpMethod

        // returns a function able execute a action (param to fun) 
        // on the controller associated with the entity type
        let resource_controller_creator (services:IServiceRegistry) (entityType:Type) (routeMatch:RouteMatch) (context:HttpContextBase) paramCallback =
            // todo: caching
            let template = typedefof<ODataEntitySubController<_>>
            let concrete = template.MakeGenericType([|entityType|])
            let spec = PredicateControllerCreationSpec(fun t -> concrete.IsAssignableFrom(t))
            let prototype = services.ControllerProvider.CreateController(spec)
            if prototype <> null then
                let executor = services.ControllerExecutorProvider.CreateExecutor(prototype)
                System.Diagnostics.Debug.Assert ( executor <> null && executor :? ODataEntitySubControllerExecutor )

                let odataExecutor = executor :?> ODataEntitySubControllerExecutor
                odataExecutor.GetParameterCallback <- Func<Type,obj>(paramCallback)
                (fun action -> let result = executor.Execute(action, prototype, routeMatch, context)
                               // if the return is an empty result, we treat it as null
                               if result <> null && result :? EmptyResult 
                               then (null, true) else (result, true))
            else (fun _ -> (null, false))

        let tryResolveParamValue (paramType:Type) isCollection (rt:ResourceType) (value:obj) =
            let entryType =
                let found = paramType.FindInterfaces(TypeFilter(fun t o -> (o :?> Type).IsAssignableFrom(t)), typedefof<IEnumerable<_>>) 
                if found.Length = 0 
                then paramType
                else found.[0].GetGenericArguments().[0]

            // if param is Model<T>
            if paramType.IsGenericType && paramType.GetGenericTypeDefinition() = typedefof<Model<_>> 
            then 
                Activator.CreateInstance ((typedefof<Model<_>>).MakeGenericType(paramType.GetGenericArguments()), [|value|])
            elif entryType <> paramType && paramType.IsAssignableFrom(entryType) then
                value
            else
                null

        member x.Model = model
        member internal x.MetadataProvider = _provider
        member internal x.MetadataProviderWrapper = _wrapper

        member x.Process(services:IServiceRegistry, httpMethod:string, greedyMatch:string, 
                         routeMatch:RouteMatch, context:HttpContextBase) = 
            
            let request = context.Request
            let response = context.Response
            response.AddHeader("DataServiceVersion", "2.0")

            let writer = response.Output
            let qs = request.Url.Query
            let baseUri = routeMatch.Uri
            let requestContentType = request.ContentType

            let invoke_controller (action:string) isCollection (rt:ResourceType) o optional =
                let paramCallback = fun (t:Type) -> tryResolveParamValue t isCollection rt o
                let actionExecutor = resource_controller_creator services rt.InstanceType routeMatch context paramCallback
                let result, executed = actionExecutor action 
                
                if not optional && not executed then
                    failwith "Non existent controller or action not found. Entity: %O action: %s. Make sure there's a controller inheriting from ODataEntitySubController" rt.InstanceType action
                else 
                    if result = null then true
                    else 
                        // todo: execute result?
                        false
            let invoke_controller_key = invoke_controller 

            let callbacks = {
                    accessSingle = Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Access" false rt o true);
                    accessMany = Func<ResourceType,IEnumerable,bool>(fun rt o -> invoke_controller "AccessMany" true rt o true);
                    create = Func<ResourceType,obj,bool * string>(fun rt o -> invoke_controller_key "Create" false rt o false, "");
                    update = Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Update" false rt o false);
                    remove = Func<ResourceType,obj,bool>(fun rt o -> invoke_controller "Remove" false rt o false);
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


