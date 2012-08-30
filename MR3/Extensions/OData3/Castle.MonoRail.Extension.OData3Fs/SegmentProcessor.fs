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
    open System.Collections
    open System.Collections.Specialized
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library



    module SegmentProcessor = 
        begin
            (*
            type ResponseParameters with 
                member x.SetStatus(code:int, desc:string) = 
                    x.httpStatus <- code
                    x.httpStatusDesc <- desc
            *)
                
            let internal emptyResponse = { 
                QItems = null; SingleResult = null; 
                FinalResourceUri=null; PropertiesToExpand = HashSet() 
            }

            let (|HttpGet|HttpPost|HttpPut|HttpDelete|HttpMerge|HttpHead|) (arg:string) = 
                match arg.ToUpperInvariant() with 
                | "GET"   -> HttpGet
                | "POST"  -> HttpPost
                | "PUT"   -> HttpPut
                | "MERGE" -> HttpMerge
                | "HEAD"  -> HttpHead
                | "DELETE"-> HttpDelete
                | _ -> failwithf "Could not parse method %s" arg

            let internal createSetting (serviceUri) (format) = 
                let messageWriterSettings = 
                    ODataMessageWriterSettings(BaseUri = serviceUri,
                                               Version = Nullable(Microsoft.Data.OData.ODataVersion.V3),
                                               Indent = true,
                                               CheckCharacters = false,
                                               DisableMessageStreamDisposal = false
                                              )
                
                messageWriterSettings.SetContentType(format)
                // messageWriterSettings.EnableWcfDataServicesServerBehavior(provider.IsV1Provider);
                // messageWriterSettings.SetContentType(acceptHeaderValue, acceptCharSetHeaderValue);
                messageWriterSettings

            let internal serialize_directory op hasMoreSegments (previous:UriSegment) writer baseUri metadataProviderWrapper (response:ODataResponseMessage) = 
                System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
                System.Diagnostics.Debug.Assert (not hasMoreSegments, "needs to be the only segment")
            
                match op with 
                | RequestOperation.Get ->
                    // response.contentType <- "application/xml;charset=utf-8"
                    // AtomServiceDocSerializer.serialize (writer, baseUri, metadataProviderWrapper, response.contentEncoding)
                    ()
                | _ -> failwithf "Unsupported operation %O at this level" op


            let internal serialize_metadata model op (previous:UriSegment) baseUri (response:ODataResponseMessage) = 
                System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

                match op with 
                | RequestOperation.Get ->
                    response.SetHeader("Content-Type", "application/xml;charset=utf-8")
                    
                    let settings = createSetting baseUri ODataFormat.Metadata

                    use writer = new ODataMessageWriter(response, settings, model)
                    writer.WriteMetadataDocument()
                    ()

                | _ -> failwithf "Unsupported operation %O at this level" op

            
            let Process (model:IEdmModel)
                        (op:RequestOperation) 
                        (segments:UriSegment[]) (meta:MetaSegment) (metaQueries:MetaQuerySegment[])
                        (ordinaryParams:NameValueCollection) 
                        // (callbacks:ProcessorCallbacks) 
                        (request:ODataRequestMessage) (response:ODataResponseMessage) = 
            
                // binds segments, delegating to SubController if they exist. 
                // for post, put, delete, merge
                //   - deserialize
                //   - process
                // for get operations
                //   - serializes results 
                // in case of exception, serialized error is sent

                // let model = request.model
                // let baseUri = request.baseUri
                // let writer = response.writer
                let parameters = List<Type * obj>()
                let lastSegment = segments.[segments.Length - 1]


                let rec rec_process (index:int) (previous:UriSegment) (result:ResponseToSend) =
                    let shouldContinue = ref true

                    if index < segments.Length then
                        let container, prevRt, containerUri = 
                            match previous with 
                            | UriSegment.EntitySet d -> d.SingleResult, d.ReturnType, d.Uri
                            | UriSegment.ComplexType d 
                            | UriSegment.PropertyAccess d -> d.SingleResult, d.ReturnType, d.Uri
                            | _ -> null, null, null
                    
                        // builds list of contextual parameters. used when calling back controllers
                        if container <> null then parameters.Add (prevRt.TargetType, container)

                        let hasMoreSegments = index + 1 < segments.Length
                        let segment = segments.[index]

                        let toSerialize = 
                            match segment with 
                            | UriSegment.ServiceDirectory -> 
                                // serialize_directory op hasMoreSegments previous request.Url response
                                emptyResponse

                            // | UriSegment.ActionOperation actionOp -> 
                            //    process_operation actionOp callbacks shouldContinue request response parameters
                            // | UriSegment.RootServiceOperation -> emptyResponse

                            | UriSegment.ComplexType d -> // | UriSegment.PropertyAccess d -> 
                                // process_item_property op container d previous hasMoreSegments model callbacks shouldContinue request response parameters
                                emptyResponse

                            | UriSegment.EntitySet d -> 
                                // process_entityset op d previous hasMoreSegments model callbacks shouldContinue request response parameters
                                emptyResponse

                            | UriSegment.PropertyAccess d -> 
                                // process_collection_property op container d previous hasMoreSegments model callbacks request response parameters shouldContinue 
                                emptyResponse

                            | _ -> Unchecked.defaultof<ResponseToSend>

                        if !shouldContinue 
                        then rec_process (index+1) segment toSerialize 
                        else result

                    else result

                let result = 
                    // process segments recursively. 
                    let navResult = rec_process 0 UriSegment.Nothing emptyResponse 
                    match meta with 
                    | MetaSegment.Nothing ->  
                        navResult
                    | MetaSegment.Metadata -> 
                     // serialize_metadata model op (previous:UriSegment) baseUri (response:ODataResponseMessage)
                        serialize_metadata model op lastSegment request.Url response
                        emptyResponse
                    | MetaSegment.Value -> 
                        () 
                        // process_operation_value lastSegment navResult response
                        emptyResponse // remove this
                    | _ -> failwithf "Unsupported meta instruction %O" meta

                let formatOverrider : Ref<String> = ref null

                // I'm starting to think that ordering may be important here:
                // select > expand > everything else
                for metaQuery in metaQueries do
                    match metaQuery with 
                    | MetaQuerySegment.Select exp ->
                        ()
                    | MetaQuerySegment.Filter exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            // apply_filter result exp
                            ()
                    | MetaQuerySegment.OrderBy exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            // apply_orderby result exp
                            ()
                    | MetaQuerySegment.Expand exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            // apply_expand result exp
                            ()
                    | MetaQuerySegment.Format fmt ->
                        formatOverrider := fmt
                    | MetaQuerySegment.InlineCount cf ->
                        ()
                    | MetaQuerySegment.Skip howMany ->
                        ()
                    | MetaQuerySegment.Top count ->
                        ()
                    | _ -> failwithf "Unsupported metaQuery instruction %O" metaQuery


                ()
                (*
                // we ultimately need to serialize a result back
                if result <> emptyResponse then 
                    if response.contentType = null then 
                        ()
                        // response.contentType <- callbacks.negotiateContent.Invoke( result.SingleResult <> null )
                    () // serialize_result !formatOverrider result request response result.FinalResourceUri 
                
                *)

        end

