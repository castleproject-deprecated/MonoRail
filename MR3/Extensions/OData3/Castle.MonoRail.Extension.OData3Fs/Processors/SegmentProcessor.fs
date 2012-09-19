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

            let private apply_filter (response:ResponseToSend) (rawExpression:string) = 
                let ast = QueryExpressionParser.parse_filter rawExpression
                let typedAst = QuerySemanticAnalysis.analyze_and_convert_for_ref ast response.EdmType

                if response.QItems <> null then 
                    response.QItems <- (AstLinqTranslator.apply_queryable_filter response.EdmType.Definition response.QItems typedAst) :?> IQueryable
            
            let private apply_orderby (response:ResponseToSend) (rawExpression:string) = 
                let exps = QueryExpressionParser.parse_orderby rawExpression
                let typedNodes = QuerySemanticAnalysis.analyze_and_convert_orderby exps response.EdmType

                if response.QItems <> null then 
                    response.QItems <- (AstLinqTranslator.apply_queryable_orderby response.EdmType.Definition response.QItems typedNodes) :?> IQueryable
            
            let private apply_expand (response:ResponseToSend) (rawExpression:string) = 
                let exps = QueryExpressionParser.parse_expand rawExpression
                QuerySemanticAnalysis.analyze_and_convert_expand exps response.EdmType response.PropertiesToExpand


            let internal internal_process (op) (segments:UriSegment[]) callbacks meta edmModel odataModel 
                                          serializer
                                          (request:ODataRequestMessage) (response:ODataResponseMessage) = 

                let parameters = List<Type * obj>()
                let lastSegment = segments.[segments.Length - 1]

                let create_processor (segment) parameters : ODataSegmentProcessor = 
                    match segment with 
                    | UriSegment.Nothing ->
                        match meta with
                        | MetaSegment.Metadata ->
                            upcast MetadataProcessor (edmModel, odataModel, callbacks, parameters, serializer, request, response)
                        | _ -> null

                    | UriSegment.EntitySet d -> 
                        upcast EntitySegmentProcessor (edmModel, odataModel, callbacks, parameters, serializer, request, response, d)

                    | UriSegment.ServiceDirectory -> 
                        upcast DirectorySegmentProcessor (edmModel, odataModel, callbacks, serializer, request, response)

                    | UriSegment.PropertyAccess d -> 
                        upcast PropertySegmentProcessor (edmModel, odataModel, callbacks, parameters, serializer, request, response, d)
                        // process_collection_property op container d previous hasMoreSegments model callbacks request response parameters shouldContinue 
                        // process_item_property op container d previous hasMoreSegments model callbacks shouldContinue request response parameters

                    (*
                    | UriSegment.FunctionOperation actionOp -> 
                        upcast ActionOperationSegmentProcessor (model)
                        // process_operation actionOp callbacks shouldContinue request response parameters
                    *)
                    | _ -> null


                let rec rec_process (index:int) (previous:UriSegment) (result:ResponseToSend) =
                    let shouldContinue = ref true

                    if index < segments.Length then
                        let container, prevRt, containerUri = 
                            match previous with 
                            | UriSegment.EntitySet d -> 
                                if d.SingleResult <> null 
                                then d.SingleResult, d.ReturnType, d.Uri
                                else d.ManyResult |> box, d.ReturnType, d.Uri
                            | UriSegment.ComplexType d 
                            | UriSegment.PropertyAccess d -> d.SingleResult, d.ReturnType, d.Uri
                            | _ -> null, null, null
                    
                        // builds list of contextual parameters. used when calling back controllers
                        if container <> null then parameters.Add (prevRt.Definition.TargetType, container)

                        let hasMoreSegments = index + 1 < segments.Length
                        let segment = segments.[index]

                        let toSerialize = 
                            let processor = create_processor (segment) parameters
                            if processor <> null 
                            then processor.Process (op, segment, previous, hasMoreSegments, shouldContinue, container)
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
                        emptyResponse 
                    | MetaSegment.Metadata ->  
                        // processed already
                        // process_operation_value lastSegment navResult response
                        emptyResponse // remove this
                    | _ -> failwithf "Unsupported meta instruction %O" meta

                result
                

            /// applies after result collect operations like
            /// filter, orderby, expand and etc
            let apply_result_modifiers_if_any (op:RequestOperation) (metaQueries) result = 
                let formatOverrider : Ref<ODataFormat> = ref null

                // I'm starting to think that ordering may be important here:
                // select > expand > everything else
                for metaQuery in metaQueries do
                    match metaQuery with 
                    // | MetaQuerySegment.Select exp ->
                    //     ()
                    | MetaQuerySegment.Filter exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            apply_filter result exp
                            
                    | MetaQuerySegment.OrderBy exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            apply_orderby result exp
                            
                    | MetaQuerySegment.Expand exp ->
                        if op <> RequestOperation.Update && op <> RequestOperation.Delete then
                            apply_expand result exp
                            
                    | MetaQuerySegment.Format fmt -> formatOverrider := fmt
                    | MetaQuerySegment.InlineCount cf -> ()
                    | MetaQuerySegment.Skip howMany -> ()
                    | MetaQuerySegment.Top count -> ()
                    | _ -> failwithf "Unsupported metaQuery instruction %O" metaQuery
                result, !formatOverrider

            let isDataModification op = 
                match op with 
                | RequestOperation.Get -> false
                | _ -> true

            /// binds segments, delegating to SubController if they exist. 
            /// for post, put, delete, merge
            ///   - deserialize
            ///   - process
            /// for get operations
            ///   - serializes results 
            /// in case of exception a serialized error is sent
            let Process (edmModel:IEdmModel) (odataModel:ODataModel)
                        (op:RequestOperation) 
                        (segments:UriSegment[]) (meta:MetaSegment) (metaQueries:MetaQuerySegment[])
                        (ordinaryParams:NameValueCollection) 
                        (callbacks:ProcessorCallbacks) 
                        (request:ODataRequestMessage) (response:ODataResponseMessage) 
                        (serialization:PayloadSerializer) = 
            
                try
                    let result = internal_process op segments callbacks meta edmModel odataModel serialization request response

                    let modifiedResult, formatOverride = 
                        if isDataModification op then
                            // Prefer : return-no-content | return-content 
                            let prefer = request.GetHeader("Prefer")
                            ()

                            (* 
                            In response to a Data Modification or Action request containing a Prefer header, 
                            the service may include a Preference-Applied response header to specify the prefer 
                            header value that was honored.

                            If the service has returned content in response to a request including a Prefer 
                            header with a value of return-content, it MAY include a Preference-Applied response 
                            header with a value of return-content.

                            If the service has returned content in response to a request including a Prefer 
                            header with a value of return-content, it MAY include a Preference-Applied response 
                            header with a value of return-no-content.
                            *)
                            result, null
                        else
                            let result = apply_result_modifiers_if_any op metaQueries result
                            result

                    if modifiedResult <> emptyResponse then 
                        if response.ContentType = null then 
                            response.ContentType <- callbacks.negotiateContent.Invoke( result.SingleResult <> null )
                            
                        serialization.Serialize(formatOverride, result, request, response)

                with 
                | exc ->
                    if response.Status >= 200 && response.Status < 300 then
                        response.SetStatus(500, "Error processing request")
                    
                    response.Clear()
                    serialization.SerializeError(exc, request, response)
        end

