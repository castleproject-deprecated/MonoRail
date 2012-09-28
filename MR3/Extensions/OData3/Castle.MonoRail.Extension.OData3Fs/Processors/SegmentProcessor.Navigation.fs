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


    type NavigationSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response, d:PropertyAccessInfo) as self = 
        inherit PropertySegmentProcessorBase(edmModel, odataModel, callbacks, parameters, serializer, request, response, d)
        
        let build_responseToSend_for_coll (items) (kind) = 
            System.Diagnostics.Debug.Assert( d.ReturnType.IsCollection() )
            { Kind = kind
              EdmContainer = d.container
              EdmEntSet = null
              EdmEntityType = d.EdmEntityType
              EdmReturnType = d.ReturnType
              QItems = items
              SingleResult = null
              FinalResourceUri = d.Uri
              EdmProperty = d.Property
              EdmFunctionImport = null
              PropertiesToExpand = HashSet() }

        let build_responseToSend_for_single  (item) (kind) = 
            System.Diagnostics.Debug.Assert( not <| d.ReturnType.IsCollection() )
            { Kind = kind
              EdmContainer = d.container
              EdmEntSet = null
              EdmEntityType = d.EdmEntityType
              EdmReturnType = d.ReturnType
              QItems = null
              SingleResult = item
              FinalResourceUri = d.Uri
              EdmProperty = d.Property
              EdmFunctionImport = null
              PropertiesToExpand = HashSet() }


        let process_single op segment previous hasMoreSegments shouldContinue container = 

            if op = RequestOperation.Get || (hasMoreSegments && op = RequestOperation.Update) then
                let propValue = self.GetPropertyValue( container, d.Property )
                
                build_responseToSend_for_single propValue ODataPayloadKind.Entry
            else
                failwithf "Unsupported operation %O at this level" op


        let process_collection op segment previous hasMoreSegments shouldContinue container = 
            
            if op = RequestOperation.Get then
                let propValue = self.GetPropertyValue( container, d.Property )
                let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()

                // should be normalized to single type instead of collection (???)
                // callbacks.Auth(d.ReturnType, parameters, collAsQueryable)

                let collAsQueryable = 
                    let interceptedVal = callbacks.InterceptMany(d.ReturnType.Definition, parameters, collAsQueryable)
                    if interceptedVal <> null
                    then interceptedVal.AsQueryable()
                    else collAsQueryable

                build_responseToSend_for_coll (collAsQueryable) ODataPayloadKind.Feed

            elif op = RequestOperation.Create then

                let item = serializer.Deserialize(d.EdmEntityType, request)

                // TODO: missing auth

                let succ = callbacks.Create(d.EdmEntityType.Definition, parameters, item)
                if succ then
                    response.StatusCode <- 201 // Created
                    // not enough info to build location
                    // response.location <- Uri(request.baseUri, d.Uri.OriginalString + "(" + key + ")").AbsoluteUri

                    { Kind = ODataPayloadKind.Entry
                      EdmContainer = d.container
                      EdmEntSet = null
                      EdmEntityType = d.EdmEntityType
                      EdmReturnType = d.ReturnType
                      QItems = null; SingleResult = item; 
                      FinalResourceUri = d.Uri; EdmProperty = null; EdmFunctionImport = null
                      PropertiesToExpand = HashSet() }
                else 
                    response.StatusCode <- 501  // Not Implemented
                    shouldContinue := false
                    emptyResponse

            else
                failwithf "Unsupported operation %O at this level" op


        let process_collection_key op segment previous hasMoreSegments shouldContinue container key = 

            let propValue = self.GetPropertyValue( container, d.Property )
            let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
            let value = self.GetSingleResult ((d.ReturnType.Definition :?> IEdmEntityType), collAsQueryable, shouldContinue, d.Key)

            if value = null then
                response.StatusCode <- 404
                shouldContinue := false
                emptyResponse

            else

                if op = RequestOperation.Get then
                    System.Diagnostics.Debug.Assert ( d.ReturnType.IsEntity() ) 

                    if value = null then
                        response.StatusCode <- 404
                        shouldContinue := false
                        emptyResponse
                    else
                        build_responseToSend_for_single value ODataPayloadKind.Entry
            
                else
                    let update item = 
                        let succ = callbacks.Update(d.EdmEntityType.Definition, parameters, item)
                        if succ 
                        then 
                            response.StatusCode <- 204 // No Content
                            build_responseToSend_for_single item ODataPayloadKind.Unsupported
                        else 
                            response.StatusCode <- 501 // Not Implemented
                            shouldContinue := false
                            emptyResponse

                    match op with 
                    | RequestOperation.Merge ->

                        let item = serializer.Deserialize(value, d.EdmEntityType, request)
                        update item
                        
                    | RequestOperation.Update -> 
                        
                        let item = serializer.Deserialize(d.EdmEntityType, request)
                        update item

                    | RequestOperation.Delete -> 
                        
                        if callbacks.Remove(d.EdmEntityType.Definition, parameters, value) then 
                            response.StatusCode <- 204 // No Content
                            build_responseToSend_for_single value ODataPayloadKind.Unsupported
                        else 
                            response.StatusCode <- 501 // Not Implemented
                            shouldContinue := false
                            emptyResponse
                        
                    | _ ->
                        failwithf "Unsupported operation %O at this level" op
            

        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue, container) = 
            
            if d.Property.Type.IsCollection() then
                if d.Key <> null 
                then process_collection_key op segment previous hasMoreSegments shouldContinue container d.Key
                else process_collection op segment previous hasMoreSegments shouldContinue container
            else
                process_single op segment previous hasMoreSegments shouldContinue container
            
            
