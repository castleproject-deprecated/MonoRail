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
    mutable httpStatusDesc : string;
    mutable location : string;
}

module SegmentProcessor = 
    begin
        type ResponseParameters with 
            member x.SetStatus(code:int, desc:string) = 
                x.httpStatus <- code
                x.httpStatusDesc <- desc
                
        let internal emptyResponse = { QItems = null; EItems = null; SingleResult = null; ResType = null; FinalResourceUri=null; ResProp = null }

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

        let private assert_entitytype_without_entityset op (rt:ResourceType) (model:ODataModel) = 
            if rt.ResourceTypeKind <> ResourceTypeKind.EntityType then 
                failwithf "Unsupported operation %O" op
            match model.GetRelatedResourceSet(rt) with
            | Some rs -> failwithf "Unsupported operation %O" op
            | _ -> ()


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

        let internal serialize_result (reply:ResponseToSend) (request:RequestParameters) (response:ResponseParameters) (containerUri:Uri) = 
            let s = SerializerFactory.Create(response.contentType) 
            let wrapper = request.wrapper

            s.Serialize(reply, wrapper, request.baseUri, containerUri, response.writer, response.contentEncoding)

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


        let internal process_collection_property op container (p:PropertyAccessInfo) (previous:UriSegment) hasMoreSegments 
                                                 (model:ODataModel) (callbacks:ProcessorCallbacks) 
                                                 (request:RequestParameters) (response:ResponseParameters)
                                                 (shouldContinue:Ref<bool>) =  
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            if op = SegmentOp.View || (hasMoreSegments && op = SegmentOp.Update) then
                let value = (get_property_value container p.Property ) :?> IEnumerable
                if callbacks.accessMany.Invoke(p.ResourceType, value) then 
                    p.ManyResult <- value 
                    { ResType = p.ResourceType; 
                      QItems = null; EItems = value; SingleResult = null; 
                      FinalResourceUri = p.Uri; ResProp = p.Property }
                else emptyResponse

            else
                match op with 
                | SegmentOp.Create -> 
                    
                    assert_entitytype_without_entityset op p.ResourceType model

                    let input = deserialize_input p.ResourceType request

                    let succ= callbacks.create.Invoke(p.ResourceType, input)
                    if succ then
                        response.SetStatus(201, "Created")
                        // we dont have enough data to build it
                        // response.location <- Uri(request.baseUri, p.Uri.OriginalString + "(" + key + ")").AbsoluteUri

                        { ResType = p.ResourceType; 
                          QItems = null; EItems = null; SingleResult = input; 
                          FinalResourceUri = p.Uri; ResProp = null }
                    else 
                        shouldContinue := false
                        emptyResponse

                | _ -> failwithf "Unsupported operation %O" op


        let internal process_item_property op container (p:PropertyAccessInfo) (previous:UriSegment) hasMoreSegments 
                                           (model:ODataModel) (callbacks:ProcessorCallbacks) (shouldContinue:Ref<bool>) 
                                           (requestParams:RequestParameters) (response:ResponseParameters) =   
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            let get_property_value () = 
                let propValue = get_property_value container p.Property
                if p.Key <> null then
                    let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
                    let value = select_by_key p.ResourceType collAsQueryable p.Key 
                    value
                else propValue

            if op = SegmentOp.View || (hasMoreSegments && op = SegmentOp.Update) then

                let finalValue = get_property_value ()

                if callbacks.accessSingle.Invoke(p.ResourceType, finalValue) then 
                    p.SingleResult <- finalValue
                    { ResType = p.ResourceType; 
                        QItems = null; EItems = null; SingleResult = finalValue; 
                        FinalResourceUri = p.Uri; ResProp = p.Property }
                else emptyResponse

            else
                System.Diagnostics.Debug.Assert (not hasMoreSegments)

                match op with
                | SegmentOp.Update -> 

                    if p.Property.IsOfKind(ResourcePropertyKind.Primitive) then 
                        // if primitive... 
                        raise(NotImplementedException("Update for property is not supported yet"))
                    
                    elif p.Property.IsOfKind(ResourcePropertyKind.ResourceSetReference) || 
                         p.Property.IsOfKind(ResourcePropertyKind.ResourceReference) then 
                        
                        // only supported for the case below, otherwise one should use $link instead
                        assert_entitytype_without_entityset op p.ResourceType model 

                        let finalValue = get_property_value ()

                        if callbacks.update.Invoke(p.ResourceType, finalValue) then 
                            response.SetStatus(204, "No Content")
                        
                        emptyResponse
                    
                    else failwithf "Operation not supported for this entity type"
                    
                | SegmentOp.Delete -> 

                    if p.Property.IsOfKind(ResourcePropertyKind.Primitive) then 
                        failwithf "Cannot delete a primitive value in a property"
                    
                    elif p.Property.IsOfKind(ResourcePropertyKind.ResourceSetReference) || 
                         p.Property.IsOfKind(ResourcePropertyKind.ResourceReference) then 
                        
                        // only supported for the case below, otherwise one should use $link instead
                        assert_entitytype_without_entityset op p.ResourceType model 

                        let finalValue = get_property_value ()

                        if callbacks.remove.Invoke(p.ResourceType, finalValue) then 
                            response.SetStatus(204, "No Content")

                        emptyResponse
                    
                    else failwithf "Operation not supported for this resource type"

                | _ -> failwithf "Operation not supported for this resource type"


        let internal process_entityset op (d:EntityAccessInfo) (previous:UriSegment) hasMoreSegments 
                                       (model:ODataModel) (callbacks:ProcessorCallbacks) (shouldContinue:Ref<bool>) 
                                       (request:RequestParameters) (response:ResponseParameters) = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
                        
            // System.Diagnostics.Debug.Assert (not hasMoreSegments)

            match op with 
            | SegmentOp.View ->
                // acceptable next segments: $count, $orderby, $top, $skip, $format, $inlinecount
                
                let value = model.GetQueryable (d.ResSet)
                if callbacks.accessMany.Invoke(d.ResourceType, value) then 
                    d.ManyResult <- value
                    { ResType = d.ResourceType; QItems = value; EItems = null; SingleResult = null; FinalResourceUri = d.Uri; ResProp = null }
                else 
                    shouldContinue := false
                    emptyResponse

            | SegmentOp.Create -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)

                let item = deserialize_input d.ResourceType request

                let succ = callbacks.create.Invoke(d.ResourceType, item)
                if succ then
                    response.SetStatus(201, "Created")
                    // not enough info to build location
                    // response.location <- Uri(request.baseUri, d.Uri.OriginalString + "(" + key + ")").AbsoluteUri

                    { ResType = d.ResourceType; 
                      QItems = null; EItems = null; SingleResult = item; 
                      FinalResourceUri = d.Uri; ResProp = null }
                else 
                    shouldContinue := false
                    emptyResponse


            | _ -> failwithf "Unsupported operation %O" op
            
        
        let internal process_entityset_single op (d:EntityAccessInfo) (previous:UriSegment) hasMoreSegments 
                                              (model:ODataModel) (callbacks:ProcessorCallbacks) (shouldContinue:Ref<bool>) 
                                              (request:RequestParameters) (response:ResponseParameters) = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

            let get_single_result () = 
                let wholeSet = model.GetQueryable (d.ResSet)
                select_by_key d.ResourceType wholeSet d.Key


            if op = SegmentOp.View || hasMoreSegments then
                if not hasMoreSegments then
                    System.Diagnostics.Debug.Assert (not (op = SegmentOp.Delete), "should not be delete")

                let singleResult = get_single_result ()

                if callbacks.accessSingle.Invoke(d.ResourceType, singleResult) then 
                    //if intercept_single op singleResult d.ResourceType shouldContinue then
                    d.SingleResult <- singleResult
                    { ResType = d.ResourceType; QItems = null; EItems = null; SingleResult = singleResult; FinalResourceUri = d.Uri; ResProp = null }
                else
                    shouldContinue := false
                    emptyResponse

            else 
            
                match op with 
                | SegmentOp.Update -> 

                    let item = deserialize_input d.ResourceType request

                    let succ = callbacks.update.Invoke(d.ResourceType, item)
                    if succ then
                        response.SetStatus(204, "No Content")
                    else shouldContinue := false
                        
                    emptyResponse

                | SegmentOp.Delete -> 
                    // http://www.odata.org/developers/protocols/operations#DeletingEntries
                    // Entries are deleted by executing an HTTP DELETE request against a URI that points at the Entry. 
                    // If the operation executed successfully servers should return 200 (OK) with no response body.
                    let single = get_single_result()
                    
                    if callbacks.remove.Invoke(d.ResourceType, single) then 
                        response.SetStatus(204, "No Content")
                    else shouldContinue := false
                        
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

        let private process_operation_value hasMoreSegments (previous:UriSegment) (result:ResponseToSend) (response:ResponseParameters) = 
            if hasMoreSegments then raise(InvalidOperationException("$value cannot be followed by more segments"))
            if result = emptyResponse || result.SingleResult = null 
               || result.ResProp = null 
               || not <| result.ResProp.IsOfKind(ResourcePropertyKind.Primitive) then 
                raise(InvalidOperationException("$value can only operate if a previous segment produced a primitive value"))
            
            // change the response type
            response.contentType <- "text/plain"
            
            // return the exact same result as the previous
            result


        let public Process (op:SegmentOp) (segments:UriSegment[]) (callbacks:ProcessorCallbacks) 
                           (request:RequestParameters) (response:ResponseParameters) = 
            
            // missing support for operations, value, filters, links, batch, ...

            // binds segments, delegating to SubController if they exist. 
            // for post, put, delete, merge
            //   - deserialize
            //   - process
            // for get operations
            //   - serializes results 
            // in case of exception, serialized error is sent

            let model = request.model
            let baseUri = request.baseUri
            let writer = response.writer
            do response.contentType <- resolveResponseContentType segments request.accept

            let rec rec_process (index:int) (previous:UriSegment) (result:ResponseToSend) =
                let shouldContinue = ref true

                if index < segments.Length then
                    let container, prevRt, containerUri = 
                        match previous with 
                        | UriSegment.EntityType d -> d.SingleResult, d.ResourceType, d.Uri
                        | UriSegment.ComplexType d 
                        | UriSegment.PropertyAccessSingle d -> d.SingleResult, d.ResourceType, d.Uri
                        | _ -> null, null, null

                    let hasMoreSegments = index + 1 < segments.Length
                    let segment = segments.[index]

                    let toSerialize = 
                        match segment with 
                        | UriSegment.Meta m -> 
                            match m with 
                            | MetaSegment.Value ->
                                process_operation_value hasMoreSegments previous result response
                                
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
                            process_entityset op d previous hasMoreSegments model callbacks shouldContinue request response

                        | UriSegment.EntityType d -> 
                            process_entityset_single op d previous hasMoreSegments model callbacks shouldContinue request response

                        | UriSegment.PropertyAccessCollection d -> 
                            process_collection_property op container d previous hasMoreSegments model callbacks request response shouldContinue 

                        | UriSegment.ComplexType d | UriSegment.PropertyAccessSingle d -> 
                            process_item_property op container d previous hasMoreSegments model callbacks shouldContinue request response

                        | _ -> Unchecked.defaultof<ResponseToSend>

                    if !shouldContinue 
                    then rec_process (index+1) segment toSerialize 
                    else result

                else result

            // process segments recursively. 
            // we ultimately need to serialize a result back
            let result = rec_process 0 UriSegment.Nothing emptyResponse 
            
            if result <> emptyResponse then 
                serialize_result result request response result.FinalResourceUri 

    end

