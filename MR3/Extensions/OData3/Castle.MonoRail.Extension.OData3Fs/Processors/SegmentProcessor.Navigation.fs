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
        
        let build_responseToSend (item) (kind) = 
            { Kind = kind
              EdmContainer = d.container
              EdmEntSet = null
              EdmType = d.ReturnType
              QItems = null
              SingleResult = item
              FinalResourceUri = d.Uri
              EdmProperty = d.Property
              PropertiesToExpand = HashSet() }

        let process_single op segment previous hasMoreSegments shouldContinue container = 
            emptyResponse

        let process_collection op segment previous hasMoreSegments shouldContinue container = 
            emptyResponse

        let process_collection_key op segment previous hasMoreSegments shouldContinue container key = 

            if op = RequestOperation.Get || (hasMoreSegments && op = RequestOperation.Update) then
                let propValue = self.GetPropertyValue( container, d.Property )
                
                System.Diagnostics.Debug.Assert ( d.ReturnType.IsEntity() ) 

                let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
                let value = self.GetSingleResult ((d.ReturnType.Definition :?> IEdmEntityType), collAsQueryable, shouldContinue, d.Key)
                
                if value = null then
                    response.StatusCode <- 404
                    shouldContinue := false
                    emptyResponse
                else
                    build_responseToSend value ODataPayloadKind.Entry
            else
                match op with
                | RequestOperation.Update -> 
                    raise(NotImplementedException("Update for property not supported yet"))
                | _ -> failwithf "Unsupported operation %O at this level" op
            

        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue, container) = 
            
            if d.Property.Type.IsCollection() then
                if d.Key <> null 
                then process_collection_key op segment previous hasMoreSegments shouldContinue container d.Key
                else process_collection op segment previous hasMoreSegments shouldContinue container
            else
                process_single op segment previous hasMoreSegments shouldContinue container
            
            
