namespace Castle.MonoRail.Extension.OData

open System
open System.IO
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open System.ServiceModel.Syndication
open System.Text
open System.Xml
open System.Xml.Linq
open Castle.MonoRail

// http://msdn.microsoft.com/en-us/library/dd233205.aspx

type SegmentOp = 
    | View = 0 
    | Create = 1
    | Update = 3
    | Delete = 4
    // | Merge = 5

type ProcessorCallbacks = {
    accessSingle : Func<ResourceType, obj, bool>;
    accessMany : Func<ResourceType, IEnumerable, bool>;
    create : Func<ResourceType, obj, bool>;
    update : Func<ResourceType, obj, bool>;
    remove : Func<ResourceType, obj, bool>;
}

type RequestParameters = {
    model : ODataModel;
    provider : IDataServiceMetadataProvider;
    wrapper : DataServiceMetadataProviderWrapper;
    contentType: string;
    contentEncoding : Encoding;
    input: Stream;
    baseUri : Uri;
    accept: string[];
}

type ResponseParameters = {
    mutable contentType: string;
    mutable contentEncoding : Encoding;
    writer : TextWriter;
    mutable httpStatus : int;
}

type ResponseToSend = {
    mutable QItems : IQueryable;
    mutable EItems : IEnumerable;
    mutable SingleResult : obj;
    ResType : ResourceType;
}

module SegmentProcessor = 
    begin
        let internal emptyResponse = { QItems = null; EItems = null; SingleResult = null; ResType = null }

        let (|HttpGet|HttpPost|HttpPut|HttpDelete|HttpMerge|HttpHead|) (arg:string) = 
            match arg.ToUpperInvariant() with 
            | "POST"  -> HttpPost
            | "PUT"   -> HttpPut
            | "MERGE" -> HttpMerge
            | "HEAD"  -> HttpHead
            | "DELETE"-> HttpDelete
            | "GET"   -> HttpGet
            | _ -> failwithf "Could not understand method %s" arg
            
        type This = static member Assembly = typeof<This>.Assembly

        let typed_select_methodinfo = 
            let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.SegmentProcessor").GetMethod("typed_select")
            System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_select methodinfo")
            m

        let typed_select<'a> (source:IQueryable) (key:obj) (keyProp:ResourceProperty) = 
            let typedSource = source :?> IQueryable<'a>
            let parameter = Expression.Parameter(source.ElementType, "element")
            let e = Expression.Property(parameter, keyProp.Name)
            let bExp = Expression.Equal(e, Expression.Constant(key))
            let exp = Expression.Lambda(bExp, [parameter]) :?> Expression<Func<'a, bool>>
            typedSource.FirstOrDefault(exp)

        let private select_by_key (rt:ResourceType) (source:IQueryable) (key:string) =
            
            // for now support for a single key
            let keyProp = Seq.head rt.KeyProperties

            let keyVal = 
                // weak!!
                System.Convert.ChangeType(key, keyProp.ResourceType.InstanceType)

            let rtType = rt.InstanceType
            let ``method`` = typed_select_methodinfo.MakeGenericMethod([|rtType|])
            let result = ``method``.Invoke(null, [|source; keyVal; keyProp|])
            if result = null then failwithf "Lookup of entity %s for key %s failed." rt.Name key
            result

        let internal serialize_result (items:IEnumerable) (item:obj) (rt:ResourceType) (request:RequestParameters) (response:ResponseParameters) = 
            let s = SerializerFactory.Create(response.contentType) 
            
            if items <> null then 
                s.SerializeMany (request.baseUri, rt, items, response.writer, response.contentEncoding)
            else 
                s.SerializeSingle (request.baseUri, rt, item, response.writer, response.contentEncoding)

        let internal deserialize_input (rt:ResourceType) (request:RequestParameters) = 
            let s = DeserializerFactory.Create(request.contentType)

            s.DeserializeSingle (rt, new StreamReader(request.input), request.contentEncoding)
            
        let internal get_property_value (container:obj) (property:ResourceProperty) = 
            // super weak
            System.Diagnostics.Debug.Assert (container <> null)
            let containerType = container.GetType()
            let getproperty = containerType.GetProperty(property.Name)
            System.Diagnostics.Debug.Assert (getproperty <> null)
            let value = getproperty.GetValue(container, null)
            value


        let internal process_collection_property op container (p:PropertyAccessDetails) (previous:UriSegment) hasMoreSegments (model:ODataModel) (shouldContinue:Ref<bool>) =  
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            if op = SegmentOp.View || (hasMoreSegments && op = SegmentOp.Update) then
                let value = (get_property_value container p.Property ) :?> IEnumerable
                //if intercept_many op value p.ResourceType shouldContinue then
                p.ManyResult <- value 
            else
                match op with 
                | SegmentOp.Update -> 
                    // deserialize 
                    // process
                    // result
                    raise(NotImplementedException("Update for property not supported yet"))
                | _ -> failwithf "Unsupported operation %O" op


        let internal process_item_property op container (p:PropertyAccessDetails) (previous:UriSegment) hasMoreSegments (model:ODataModel) (shouldContinue:Ref<bool>) =  
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            if op = SegmentOp.View || (hasMoreSegments && op = SegmentOp.Update) then
                let propValue = get_property_value container p.Property
                if p.Key <> null then
                    let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
                    let value = select_by_key p.ResourceType collAsQueryable p.Key 
                    //if intercept_single op value p.ResourceType shouldContinue then
                    p.SingleResult <- value
                else
                    //if intercept_single op propValue p.ResourceType shouldContinue then
                    p.SingleResult <- propValue
            else
                match op with
                | SegmentOp.Update -> 
                    // if primitive... 
                    raise(NotImplementedException("Update for property not supported yet"))
                    
                // | SegmentOp.Delete -> is the property a relationship? should delete through a $link instead
                | _ -> ()


        let internal process_entityset op (d:EntityDetails) (previous:UriSegment) hasMoreSegments 
                                       (model:ODataModel) (callbacks:ProcessorCallbacks) (shouldContinue:Ref<bool>) requestParams = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
                        
            // System.Diagnostics.Debug.Assert (not hasMoreSegments)

            match op with 
            | SegmentOp.View ->
                // acceptable next segments: $count, $orderby, $top, $skip, $format, $inlinecount
                
                let value = model.GetQueryable (d.Name)
                if callbacks.accessMany.Invoke(d.ResourceType, value) then 
                    d.ManyResult <- value
                    { ResType = d.ResourceType; QItems = value; EItems = null; SingleResult = null }
                else 
                    shouldContinue := false
                    emptyResponse

            | SegmentOp.Create -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)

                let item = deserialize_input d.ResourceType requestParams

                if callbacks.create.Invoke(d.ResourceType, item) then
                    ()
                else
                    shouldContinue := false
                    
                emptyResponse

            | SegmentOp.Update -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)
                // deserialize 
                // process
                // result
                emptyResponse

            | SegmentOp.Delete -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)
                // process
                // result
                emptyResponse

            | _ -> failwithf "Unsupported operation %O" op
            
        
        let internal process_entitytype op (d:EntityDetails) (previous:UriSegment) hasMoreSegments (model:ODataModel) (shouldContinue:Ref<bool>) stream = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

            if op = SegmentOp.View || (hasMoreSegments && op = SegmentOp.Update) then
                System.Diagnostics.Debug.Assert (not (op = SegmentOp.Delete), "should not be delete")

                // if there are more segments, consider this a read
                let wholeSet = model.GetQueryable (d.Name)
                let singleResult = select_by_key d.ResourceType wholeSet d.Key
                //if intercept_single op singleResult d.ResourceType shouldContinue then
                d.SingleResult <- singleResult

                { ResType = d.ResourceType; QItems = null; EItems = null; SingleResult = singleResult }

            else
                match op with 
                | SegmentOp.Update -> 
                    // deserialize 
                    // process
                    // result
                    emptyResponse

                | SegmentOp.Delete -> 
                    // http://www.odata.org/developers/protocols/operations#DeletingEntries
                    // Entries are deleted by executing an HTTP DELETE request against a URI that points at the Entry. 
                    // If the operation executed successfully servers should return 200 (OK) with no response body.
                    
                    // process
                    // result
                    emptyResponse 

                | _ -> failwithf "Unsupported operation %O at this level" op
        

        let internal serialize_directory op hasMoreSegments (previous:UriSegment) writer baseUri metadataProviderWrapper (response:ResponseParameters) = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
            System.Diagnostics.Debug.Assert (not hasMoreSegments, "needs to be the only segment")
            
            match op with 
            | SegmentOp.View ->
                response.contentType <- "application/xml;charset=utf-8"
                AtomServiceDocSerializer.serialize (writer, baseUri, metadataProviderWrapper, response.contentEncoding)
            | _ -> failwithf "Unsupported operation %O at this level" op


        let internal serialize_metadata op hasMoreSegments (previous:UriSegment) writer baseUri metadataProviderWrapper (response:ResponseParameters) = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
            System.Diagnostics.Debug.Assert (not hasMoreSegments, "needs to be the only segment")

            match op with 
            | SegmentOp.View ->
                response.contentType <- "application/xml;charset=utf-8"
                MetadataSerializer.serialize (writer, metadataProviderWrapper, response.contentEncoding)
            | _ -> failwithf "Unsupported operation %O at this level" op

        let internal resolveResponseContentType (segments:UriSegment[]) (acceptTypes:string[]) = 
            match segments |> Array.tryPick (fun s -> match s with | UriSegment.Meta m -> (match m with | MetaSegment.Format f -> Some(f) | _ -> None ) | _ -> None) with 
            | Some f -> 
                match f.ToLowerInvariant() with 
                | "atom" -> "application/atom+xml"
                | "xml"  -> "application/xml"
                | "json" -> "application/json"
                | _ -> f
            | _ -> 
                // should be more sophisticate than this..
                if acceptTypes = null || acceptTypes.Length = 0 
                then "application/atom+xml" // defaults to atom
                else
                    if acceptTypes |> Array.exists (fun at -> at.StartsWith("*/*", StringComparison.OrdinalIgnoreCase) )
                    then "application/atom+xml" 
                    else acceptTypes.[0]


        let public Process (op:SegmentOp) (segments:UriSegment[]) (callbacks:ProcessorCallbacks) (request:RequestParameters) (response:ResponseParameters) = 
            
            // missing support for operations, value, filters, links, batch, ...

            // binds segments, delegating to SubController if they exist. 
            // for post, put, delete, merge
            //   - deserialize
            //   - process
            // for get operations
            //   - serializes results 
            // in case of exception, serialized error is sent

            let model = request.model
            let stream = request.input
            let baseUri = request.baseUri
            let writer = response.writer
            do response.contentType <- resolveResponseContentType segments request.accept

            let rec rec_process (index:int) (previous:UriSegment) (result:ResponseToSend)  =
                let shouldContinue = ref true

                if index < segments.Length then
                    let container, prevRt = 
                        match previous with 
                        | UriSegment.EntityType d -> d.SingleResult, d.ResourceType
                        | UriSegment.ComplexType d 
                        | UriSegment.PropertyAccessSingle d -> d.SingleResult, d.ResourceType
                        | _ -> null, null

                    let hasMoreSegments = index + 1 < segments.Length
                    let segment = segments.[index]

                    let toSerialize = 
                        match segment with 
                        | UriSegment.Meta m -> 
                            match m with 
                            | MetaSegment.Metadata -> 
                                serialize_metadata op hasMoreSegments previous writer baseUri request.wrapper response
                                emptyResponse
                            | _ -> failwithf "Unsupported meta instruction %O" m

                        | UriSegment.ServiceDirectory -> 
                            serialize_directory op hasMoreSegments previous writer baseUri request.wrapper response
                            emptyResponse

                        | UriSegment.ServiceOperation -> 
                            ()
                            emptyResponse

                        | UriSegment.EntitySet d -> 
                            process_entityset op d previous hasMoreSegments model callbacks shouldContinue request

                        | UriSegment.EntityType d -> 
                            process_entitytype op d previous hasMoreSegments model shouldContinue stream

                        | UriSegment.PropertyAccessCollection d -> 
                            process_collection_property op container d previous hasMoreSegments model shouldContinue 
                            emptyResponse

                        | UriSegment.ComplexType d | UriSegment.PropertyAccessSingle d -> 
                            process_item_property op container d previous hasMoreSegments model shouldContinue 
                            emptyResponse

                        | _ -> Unchecked.defaultof<ResponseToSend>

                    // if !shouldContinue then 
                    rec_process (index+1) segment toSerialize
                    // else 

                else result

            // process segments recursively. 
            // we ultimately need to serialize a result back
            let result = rec_process 0 UriSegment.Nothing emptyResponse
            
            if result <> emptyResponse then 
                let items : IEnumerable = 
                    if result.QItems <> null 
                    then upcast result.QItems 
                    else result.EItems
                let item = result.SingleResult
                let rt = result.ResType
                
                serialize_result items item rt request response

            ()
            

    end

