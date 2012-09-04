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

            let (|HttpGet|HttpPost|HttpPut|HttpDelete|HttpMerge|HttpHead|) (arg:string) = 
                match arg.ToUpperInvariant() with 
                | "GET"   -> HttpGet
                | "POST"  -> HttpPost
                | "PUT"   -> HttpPut
                | "MERGE" -> HttpMerge
                | "HEAD"  -> HttpHead
                | "DELETE"-> HttpDelete
                | _ -> failwithf "Could not parse method %s" arg


            let internal internal_process (op) (segments:UriSegment[]) meta model 
                                          (request:ODataRequestMessage) (response:ODataResponseMessage) = 
                // let model = request.model
                // let baseUri = request.baseUri
                // let writer = response.writer
                let parameters = List<Type * obj>()
                let lastSegment = segments.[segments.Length - 1]

                let create_processor (segment) : ODataSegmentProcessor = 
                    match segment with 
                    | UriSegment.Nothing ->
                        match meta with
                        | MetaSegment.Metadata ->
                            upcast MetadataProcessor (model)
                        | _ -> null

                    | UriSegment.ServiceDirectory -> 
                        upcast DirectorySegmentProcessor (model)
                        // serialize_directory op hasMoreSegments previous request.Url response

                    | UriSegment.EntitySet d -> 
                        upcast EntitySegmentProcessor (model)
                        // process_entityset op d previous hasMoreSegments model callbacks shouldContinue request response parameters

                    | UriSegment.PropertyAccess d -> 
                        upcast PropertySegmentProcessor (model)
                        // process_collection_property op container d previous hasMoreSegments model callbacks request response parameters shouldContinue 
                        // process_item_property op container d previous hasMoreSegments model callbacks shouldContinue request response parameters

                    | UriSegment.FunctionOperation actionOp -> 
                        upcast ActionOperationSegmentProcessor (model)
                    //    process_operation actionOp callbacks shouldContinue request response parameters

                    | _ -> null


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
                            let processor = create_processor (segment)
                            if processor <> null 
                            then processor.Process (op, request, response)
                            else emptyResponse

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
                    | MetaSegment.Value -> 
                        () 
                        // process_operation_value lastSegment navResult response
                        emptyResponse // remove this
                    | _ -> failwithf "Unsupported meta instruction %O" meta

                result
                

            /// applies after result collect operations like
            /// filter, orderby, expand and etc
            let apply_result_modifiers_if_any (op:RequestOperation) (metaQueries) result = 
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

                result 


            /// binds segments, delegating to SubController if they exist. 
            /// for post, put, delete, merge
            ///   - deserialize
            ///   - process
            /// for get operations
            ///   - serializes results 
            /// in case of exception a serialized error is sent
            let Process (model:IEdmModel)
                        (op:RequestOperation) 
                        (segments:UriSegment[]) (meta:MetaSegment) (metaQueries:MetaQuerySegment[])
                        (ordinaryParams:NameValueCollection) 
                        // (callbacks:ProcessorCallbacks) 
                        (request:ODataRequestMessage) (response:ODataResponseMessage) = 
            
                try
                    let result = internal_process op segments meta model request response

                    let result = apply_result_modifiers_if_any op metaQueries result

                    (*
                    if result <> emptyResponse then 
                    if response.contentType = null then 
                        ()
                        // response.contentType <- callbacks.negotiateContent.Invoke( result.SingleResult <> null )
                    () // serialize_result !formatOverrider result request response result.FinalResourceUri 
                    *)

                    ()

                with 
                | exc -> 
                    ()

        end

