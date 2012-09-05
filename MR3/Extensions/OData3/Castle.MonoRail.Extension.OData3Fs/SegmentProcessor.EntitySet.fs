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


    type EntitySegmentProcessor(edmModel, odataModel, callbacks, parameters, request, response, d:EntityAccessInfo) as self = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, request, response)
        
        
        let process_single op segment previous hasMoreSegments shouldContinue = 

            if op = RequestOperation.Get || hasMoreSegments then
                if not hasMoreSegments then Diagnostics.Debug.Assert (not (op = RequestOperation.Delete), "should not be delete")

                let singleResult = self.GetSingleResult (d.EdmSet, shouldContinue, d.Key)
                d.SingleResult <- singleResult

                if singleResult = null then 
                    shouldContinue := false
                    response.SetStatus(404, "Not Found")
                    emptyResponse

                else 
                    if singleResult <> null then
                        if not hasMoreSegments && not <| callbacks.View(d.EdmEntityType, parameters, singleResult) then
                            shouldContinue := false
                    else shouldContinue := false
                    
                    if !shouldContinue then
                        { EdmType = d.EdmEntityType
                          QItems = null
                          SingleResult = singleResult
                          FinalResourceUri = d.Uri
                          EdmProperty = null
                          PropertiesToExpand = HashSet() }
                    else emptyResponse

            else 
                let single = self.GetSingleResult(d.EdmSet, shouldContinue, d.Key)

                match op with 
                | RequestOperation.Update -> 
                    // runs auth
                    if single <> null then 
                        // todo: shouldn't it deserialize into 'single'?
                        let item = Serialization.deserialize_input d.EdmEntityType request
                        let succ = callbacks.Update(d.EdmEntityType, parameters, item)
                        if succ 
                        then response.SetStatus(204, "No Content")
                        else 
                            response.SetStatus(501, "Not Implemented")
                            shouldContinue := false

                | RequestOperation.Delete -> 
                    // http://www.odata.org/developers/protocols/operations#DeletingEntries
                    // Entries are deleted by executing an HTTP DELETE request against a URI that points at the Entry. 
                    // If the operation executed successfully servers should return 200 (OK) with no response body.
                    
                    if single <> null then 
                        if callbacks.Remove(d.EdmEntityType, parameters, single) then 
                            response.SetStatus(204, "No Content")
                        else 
                            response.SetStatus(501, "Not Implemented")
                            shouldContinue := false

                | _ -> failwithf "Unsupported operation %O at this level" op

                emptyResponse


        let process_collection op segment previous hasMoreSegments shouldContinue = 
            
            match op with 
            | RequestOperation.Get ->
                // acceptable next segments: $count, $orderby, $top, $skip, $format, $inlinecount
                
                let values = self.GetSetResult (d.EdmSet, shouldContinue)
                d.ManyResult <- values

                if values <> null then
                    if not hasMoreSegments && not <| callbacks.View( d.EdmEntityType, parameters, values ) then
                        shouldContinue := false

                // remember: this ! is not NOT, it's a de-ref
                if !shouldContinue then
                    { EdmType = d.EdmEntityType
                      QItems = values; SingleResult = null; 
                      FinalResourceUri = d.Uri; EdmProperty = null; 
                      PropertiesToExpand = HashSet() }
                else emptyResponse 


            | RequestOperation.Create -> 
                System.Diagnostics.Debug.Assert (not hasMoreSegments)

                let item = Serialization.deserialize_input d.EdmEntityType request

                let succ = callbacks.Create(d.EdmEntityType, parameters, item)
                if succ then
                    // response.SetStatus(201, "Created")
                    // not enough info to build location
                    // response.location <- Uri(request.baseUri, d.Uri.OriginalString + "(" + key + ")").AbsoluteUri

                    { EdmType = d.EdmEntityType; 
                      QItems = null; SingleResult = item; 
                      FinalResourceUri = d.Uri; EdmProperty = null; 
                      PropertiesToExpand = HashSet() }
                else 
                    // response.SetStatus(501, "Not Implemented")
                    shouldContinue := false
                    emptyResponse

            | _ -> failwithf "Unsupported operation for entity set segment %O" op


        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue) = 

            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

            if d.Key <> null then
                process_single op segment previous hasMoreSegments shouldContinue
            else
                process_collection op segment previous hasMoreSegments shouldContinue
                
            




            
            