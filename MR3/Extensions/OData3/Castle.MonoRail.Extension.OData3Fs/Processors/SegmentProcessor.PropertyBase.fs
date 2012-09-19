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


    
    [<AbstractClass>]
    type PropertySegmentProcessorBase(edmModel, odataModel, callbacks, parameters, serializer, request, response, d:PropertyAccessInfo) = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response)

        (*        
        let get_property_value (container:obj) (property:IEdmProperty) = 
            System.Diagnostics.Debug.Assert (container <> null)
            
            match property with
            | :? TypedEdmStructuralProperty as p -> 
                p.GetValue( container )
            | :? TypedEdmNavigationProperty as p -> 
                p.GetValue( container )
            | _ ->
                failwithf "Cannot get property value from this type of property %O" property

        let select_by_key (edmType) (collection) key = 
            try
                let singleResult = AstLinqTranslator.select_by_key edmType collection key
                if singleResult <> null then 
                    (*
                    if auth_item singleResult edmEntitySet.ElementType shouldContinue
                    then 
                        let newVal = callbacks.Intercept(edmEntitySet.ElementType, callbackParameters, singleResult) 
                        if newVal <> null 
                        then newVal
                        else singleResult
                    else null
                    *)
                    singleResult
                else null
            with 
            | exc -> 
                // not found?
                null 


        let process_single op segment previous hasMoreSegments shouldContinue container =  
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            if op = RequestOperation.Get || (hasMoreSegments && op = RequestOperation.Update) then
                let propValue = get_property_value container d.Property
                if d.Key <> null then
                    System.Diagnostics.Debug.Assert ( d.ReturnType.IsEntity() ) 

                    let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
                    let value = select_by_key (d.ReturnType.Definition :?> IEdmEntityType) collAsQueryable d.Key
                    //if intercept_single op value p.ResourceType shouldContinue then
                    d.SingleResult <- value
                else
                    //if intercept_single op propValue p.ResourceType shouldContinue then
                    d.SingleResult <- propValue
            else
                match op with
                | RequestOperation.Update -> 
                    // if primitive... 
                    raise(NotImplementedException("Update for property not supported yet"))
                // | SegmentOp.Delete -> is the property a relationship? should delete through a $link instead
                | _ -> ()  


        let process_collection op segment previous hasMoreSegments shouldContinue container =  
            System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

            if op = RequestOperation.Get || (hasMoreSegments && op = RequestOperation.Update) then
                let value = (get_property_value container d.Property ) :?> IEnumerable
                //if intercept_many op value p.ResourceType shouldContinue then
                // p.ManyResult <- value 
                ()
            else
                match op with 
                | RequestOperation.Update -> 
                    // deserialize 
                    // process
                    // result
                    raise(NotImplementedException("Update for property not supported yet"))
                | _ -> failwithf "Unsupported operation %O" op


        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue, container) = 
            
            if d.ReturnType.IsCollection() then
                if d.Key <> null
                then process_single op segment previous hasMoreSegments shouldContinue container
                else process_collection op segment previous hasMoreSegments shouldContinue container
            else
                process_single op segment previous hasMoreSegments shouldContinue container
            
            emptyResponse
        *)
    