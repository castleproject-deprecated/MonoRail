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

    
    type ActionOperationSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response, d:FuncAccessInfo) = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response)
        
        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue, container) = 
            let additionalParams = List<Type * obj>( parameters )

            let process_input_param (p:IEdmFunctionParameter) = 
                let isEntity = p.Type.IsEntity()
                let isCollOfEntity = (p.Type.IsCollection() && (p.Type :?> IEdmCollectionTypeReference).ElementType().IsEntity())
                
                if (p.Mode = EdmFunctionParameterMode.In || p.Mode = EdmFunctionParameterMode.InOut) && 
                   (isEntity || isCollOfEntity) then
                   
                    let input = serializer.Deserialize(p.Type, request)
                    additionalParams.Add( p.Type.Definition.TargetType, input )

                    ()
                   
                ()

            let func = d.FunctionImport

            // process all entity type parameters
            func.Parameters |> Seq.iteri (fun i p -> if i > 0 then process_input_param p )

            let result = callbacks.Operation(d.EdmEntityType.Definition, additionalParams, d.FunctionImport.Name)

            let payloadKind, single, coll = 
                if func.ReturnType.IsCollection() && 
                    ((func.ReturnType.IsCollection() && (func.ReturnType :?> IEdmCollectionTypeReference).ElementType().IsEntity())) then

                    ODataPayloadKind.Feed, null, (result :?> IEnumerable).AsQueryable()

                elif func.ReturnType.IsEntity() then
                    ODataPayloadKind.Entry, result, null

                elif func.ReturnType.IsPrimitive() then
                    ODataPayloadKind.Value, result, null

                elif func.ReturnType.IsCollection() then
                    ODataPayloadKind.Collection, result, null

                else 
                    ODataPayloadKind.Entry, result, null

            {
                Kind = payloadKind
                QItems = coll // IQueryable
                SingleResult = single // obj
                FinalResourceUri = d.Uri
                EdmEntSet = d.EdmSet
                EdmEntityType = d.EdmEntityType
                EdmReturnType = func.ReturnType
                EdmProperty = null
                EdmContainer = d.container
                EdmFunctionImport = func
                PropertiesToExpand = HashSet()
            }
    