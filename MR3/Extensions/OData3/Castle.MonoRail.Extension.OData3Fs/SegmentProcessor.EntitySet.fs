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


    type EntitySegmentProcessor(edmModel, odataModel, callbacks, parameters, request, response, d:EntityAccessInfo) = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, request, response)
        
        
        let process_single op segment previous parameters hasMoreSegments shouldContinue = 
            ()
            
        let process_collection op segment previous parameters hasMoreSegments shouldContinue = 
            ()

        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue) = 

            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

            if d.Key <> null then
                process_single op segment previous parameters hasMoreSegments shouldContinue
            else
                process_collection op segment previous parameters hasMoreSegments shouldContinue
                
            emptyResponse


            match op with 
            | RequestOperation.Get ->
                // acceptable next segments: $count, $orderby, $top, $skip, $format, $inlinecount
                
                let values = get_values ()
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

            
            