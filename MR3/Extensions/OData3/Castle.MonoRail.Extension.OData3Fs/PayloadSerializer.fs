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
    type PayloadSerializer() = 

        abstract member SerializeMetadata : request:IODataRequestMessage * response:IODataResponseMessage -> unit
        abstract member SerializeServiceDoc : request:IODataRequestMessage * response:IODataResponseMessage -> unit
        
        abstract member SerializeFeed : models:IQueryable * edmEntSet:IEdmEntitySet * edmEntType:IEdmEntityTypeReference * request:IODataRequestMessage * response:IODataResponseMessage -> unit
        abstract member SerializeEntry : model:obj * edmEntSet:IEdmEntitySet * edmEntType:IEdmEntityTypeReference * request:IODataRequestMessage * response:IODataResponseMessage -> unit
        
        abstract member SerializeCollection : models:IQueryable * edmType:IEdmTypeReference * request:IODataRequestMessage * response:IODataResponseMessage -> unit
        abstract member SerializeProperty : model:obj * edmType:IEdmTypeReference * request:IODataRequestMessage * response:IODataResponseMessage -> unit

        abstract member SerializeValue : value:obj * edmType:IEdmTypeReference * request:IODataRequestMessage * response:IODataResponseMessage -> unit
        
        abstract member SerializeError : ``exception``:Exception * request:IODataRequestMessage * response:IODataResponseMessage -> unit
        abstract member Deserialize : edmType:IEdmTypeReference * request:IODataRequestMessage -> obj


        member x.Serialize(formatOverride:ODataFormat, toSend:ResponseToSend, request:IODataRequestMessage, response:IODataResponseMessage) = 
            
            match toSend.Kind with
            | ODataPayloadKind.Feed  ->
                x.SerializeFeed (toSend.QItems, toSend.EdmEntSet, toSend.EdmType :?> IEdmEntityTypeReference, request, response)

            | ODataPayloadKind.Entry ->
                x.SerializeEntry (toSend.SingleResult, toSend.EdmEntSet, toSend.EdmType :?> IEdmEntityTypeReference, request, response)

            | ODataPayloadKind.ServiceDocument  ->
                x.SerializeServiceDoc (request, response)

            | ODataPayloadKind.Collection ->
                let entSet = 
                    if toSend.EdmEntSet <> null 
                    then toSend.EdmEntSet
                    else 
                        System.Diagnostics.Debug.Assert(toSend.EdmType.IsEntity())
                        upcast EdmEntitySet(toSend.EdmContainer, toSend.EdmType.Definition.FName, toSend.EdmType.Definition :?> IEdmEntityType)

                // then 
                x.SerializeFeed (toSend.QItems, toSend.EdmEntSet, toSend.EdmType :?> IEdmEntityTypeReference, request, response)
                // else x.SerializeCollection (toSend.QItems, toSend.EdmType, request, response)
                
            | ODataPayloadKind.Property ->
                if toSend.EdmEntSet <> null 
                then x.SerializeEntry (toSend.QItems, toSend.EdmEntSet, toSend.EdmType :?> IEdmEntityTypeReference, request, response)
                else x.SerializeProperty (toSend.SingleResult, toSend.EdmType, request, response)

            | ODataPayloadKind.Value ->
                x.SerializeValue (toSend.SingleResult, toSend.EdmType, request, response)

            | _ -> failwithf "Dont know who to deal with payload kind %O" toSend.Kind

            ()




