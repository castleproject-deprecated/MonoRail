namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Data.OData
    open System.Data.Services.Providers
    open System.Linq
    open System.Text
    open System.Web
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.OData
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extension.OData


    /// Entry point for exposing EntitySets through OData
    [<AbstractClass>]
    type ODataEntitySubController<'TEntity when 'TEntity : not struct>() = 
        class
            // view
            // create
            // update
            // delete
        end


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

        member x.Model = model
        member internal x.MetadataProvider = _provider
        member internal x.MetadataProviderWrapper = _wrapper 

        (* 
        let resource_controller_creator (entityType:Type) =
            let template = typedefof<ODataEntitySubController<_>>
            let concrete = template.MakeGenericType([|entityType|])
            let spec = PredicateControllerCreationSpec(fun t -> concrete.IsAssignableFrom(t))
            let prototype = services.ControllerProvider.CreateController(spec)
            if prototype <> null then
                let executor = services.ControllerExecutorProvider.CreateExecutor(prototype)
                System.Diagnostics.Debug.Assert ( executor <> null && executor :? ODataEntitySubControllerExecutor )

                let odataExecutor = executor :?> ODataEntitySubControllerExecutor
                odataExecutor.GetParameterCallback <- (fun t -> null)
                (fun action -> let result = executor.Execute(action, prototype, routeMatch, context)
                               // if the return is an empty result, we treat it as null
                               if result <> null && result :? EmptyResult 
                               then null else result)
            else (fun _ -> null)
        *)

        member x.Process(services:IServiceRegistry, httpMethod:string, greedyMatch:string, 
                         routeMatch:RouteMatch, context:HttpContextBase) = 
            
            let request = context.Request
            let response = context.Response
            let writer = response.Output
            let qs = request.Url.Query
            let baseUri = routeMatch.Uri

            let requestContentType = request.ContentType

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
                    contentType = null;
                    contentEncoding = response.ContentEncoding;
                    writer = writer;
                }

            response.Headers.Add ("DataServiceVersion", "2.0")

            try
                let op = resolveHttpOperation httpMethod
                let segments = SegmentParser.parse (greedyMatch, qs, model)
                // responseContentType := resolveResponseContentType segments request.AcceptTypes

                SegmentProcessor.Process op segments requestParams responseParams

                if String.IsNullOrEmpty responseParams.contentType then
                    response.ContentType <- responseParams.contentType
                if responseParams.contentEncoding <> null then 
                    response.ContentEncoding <- responseParams.contentEncoding

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
                

            

            


