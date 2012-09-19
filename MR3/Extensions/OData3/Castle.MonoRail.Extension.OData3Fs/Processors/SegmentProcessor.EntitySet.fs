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


    type EntitySegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response, d:EntityAccessInfo) as self = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response)
        

        let build_responseToSend (item) (kind) = 
            { Kind = kind
              EdmContainer = d.container
              EdmEntSet = d.EdmSet
              EdmType = d.EdmEntityType
              QItems = null
              SingleResult = item
              FinalResourceUri = d.Uri
              EdmProperty = null
              PropertiesToExpand = HashSet() }

        
        let process_single op segment previous hasMoreSegments shouldContinue = 

            let singleResult = self.GetSingleResult (d.EdmSet, shouldContinue, d.Key)
            d.SingleResult <- singleResult

            if singleResult = null then 
                shouldContinue := false
                response.SetStatus(404, "Not Found")
                emptyResponse
            else

                if op = RequestOperation.Get || hasMoreSegments then

                    if singleResult <> null then
                        if not hasMoreSegments && not <| callbacks.View(d.EdmEntityType.Definition, parameters, singleResult) then
                            shouldContinue := false
                    else shouldContinue := false
                    
                    if !shouldContinue then
                        build_responseToSend singleResult ODataPayloadKind.Entry
                    else emptyResponse

                else 

                    match op with 
                    | RequestOperation.Merge
                    | RequestOperation.Update -> 
                        let item = serializer.Deserialize(d.EdmEntityType, request)
                        
                        // TODO: How to give a cue to the subscontroller that it should behave as merge instead of update?
                        let succ = callbacks.Update(d.EdmEntityType.Definition, parameters, item)
                        
                        if succ 
                        then 
                            response.SetStatus(204, "No Content")
                            build_responseToSend singleResult ODataPayloadKind.Entry
                        else 
                            response.SetStatus(501, "Not Implemented")
                            shouldContinue := false
                            emptyResponse

                    | RequestOperation.Delete -> 
                        if callbacks.Remove(d.EdmEntityType.Definition, parameters, single) then 
                            response.SetStatus(204, "No Content")
                            build_responseToSend singleResult ODataPayloadKind.Entry
                        else 
                            response.SetStatus(501, "Not Implemented")
                            shouldContinue := false
                            emptyResponse

                    | _ -> failwithf "Unsupported operation %O at this level" op

            


        let process_collection op segment previous hasMoreSegments shouldContinue = 
            
            match op with 
            | RequestOperation.Get ->
                // acceptable next segments: $count, $orderby, $top, $skip, $format, $inlinecount
                
                let values = self.GetSetResult (d.EdmSet, shouldContinue)
                d.ManyResult <- values

                if values <> null then
                    if not hasMoreSegments && not <| callbacks.View( d.EdmEntityType.Definition, parameters, values ) then
                        shouldContinue := false

                // remember: this ! is not NOT, it's a de-ref
                if !shouldContinue then
                    { Kind = ODataPayloadKind.Feed
                      EdmContainer = d.container
                      EdmEntSet = d.EdmSet
                      EdmType = d.EdmEntityType
                      QItems = values; SingleResult = null; 
                      FinalResourceUri = d.Uri; EdmProperty = null; 
                      PropertiesToExpand = HashSet() }
                else emptyResponse 


            | RequestOperation.Create -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)

                let item = serializer.Deserialize(d.EdmEntityType, request)

                let succ = callbacks.Create(d.EdmEntityType.Definition, parameters, item)
                if succ then
                    // response.SetStatus(201, "Created")
                    // not enough info to build location
                    // response.location <- Uri(request.baseUri, d.Uri.OriginalString + "(" + key + ")").AbsoluteUri

                    { Kind = ODataPayloadKind.Feed
                      EdmContainer = d.container
                      EdmEntSet = d.EdmSet
                      EdmType = d.EdmEntityType
                      QItems = null; SingleResult = item; 
                      FinalResourceUri = d.Uri; EdmProperty = null; 
                      PropertiesToExpand = HashSet() }
                else 
                    // response.SetStatus(501, "Not Implemented")
                    shouldContinue := false
                    emptyResponse

            | _ -> failwithf "Unsupported operation for entity set segment %O" op


        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue, container) = 
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

            if d.Key <> null
            then process_single op segment previous hasMoreSegments shouldContinue
            else process_collection op segment previous hasMoreSegments shouldContinue
                
            




            
            