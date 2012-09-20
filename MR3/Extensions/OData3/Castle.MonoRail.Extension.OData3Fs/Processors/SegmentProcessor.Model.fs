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
    open System.IO
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Castle.MonoRail


    type ProcessorCallbacks = {
        intercept       : Func<IEdmType, (Type * obj) seq, obj, obj>;
        interceptMany   : Func<IEdmType, (Type * obj) seq, IEnumerable, IEnumerable>;
        authorize       : Func<IEdmType, (Type * obj) seq, obj, bool>;
        authorizeMany   : Func<IEdmType, (Type * obj) seq, IEnumerable, bool>;
        view            : Func<IEdmType, (Type * obj) seq, obj, bool>;
        viewMany        : Func<IEdmType, (Type * obj) seq, IEnumerable, bool>;
        create          : Func<IEdmType, (Type * obj) seq, obj, bool>;
        update          : Func<IEdmType, (Type * obj) seq, obj, bool>;
        remove          : Func<IEdmType, (Type * obj) seq, obj, bool>;
        operation       : Func<IEdmType, (Type * obj) seq, string, obj>;
        negotiateContent: Func<bool, string>;
    } with
        member x.Intercept (rt, parameters, item) = 
            if x.intercept <> null
            then x.intercept.Invoke(rt, parameters, item) 
            else item
        member x.InterceptMany (rt, parameters, items) = 
            if x.interceptMany <> null
            then x.interceptMany.Invoke(rt, parameters, items) 
            else items
        member x.Auth   (rt, parameters, item) = 
            if x.authorize <> null 
            then x.authorize.Invoke(rt, parameters, item) 
            else true
        member x.Auth   (rt, parameters, items) = 
            if x.authorizeMany <> null 
            then x.authorizeMany.Invoke(rt, parameters, items) 
            else true
        member x.View   (rt, parameters, item) = 
            if x.view <> null
            then x.view.Invoke(rt, parameters, item) 
            else false
        member x.View   (rt, parameters, items) = 
            if x.viewMany <> null 
            then x.viewMany.Invoke(rt, parameters, items) 
            else false
        member x.Create (rt, parameters, item) = 
            if x.create <> null
            then x.create.Invoke(rt, parameters, item)
            else false
        member x.Update (rt, parameters, item) = 
            if x.update <> null
            then x.update.Invoke(rt, parameters, item)
            else false
        member x.Remove (rt, parameters, item) = 
            if x.remove <> null
            then x.remove.Invoke(rt, parameters, item)
            else false
        member x.Operation (rt, parameters, action) = 
            if x.operation <> null 
            then x.operation.Invoke(rt, parameters, action)
            else null



    [<AbstractClass;AllowNullLiteral>]
    type ODataSegmentProcessor (edmModel:IEdmModel, odataModel:ODataModel, 
                                callbacks:ProcessorCallbacks, callbackParameters:List<Type*obj>, 
                                serializer:PayloadSerializer, 
                                request:IODataRequestMessage, response:IODataResponseMessage) =  

        let auth_item (item:obj) (edmType) (shouldContinue) = 
            let succ = callbacks.Auth(edmType, callbackParameters, item) 
            if not succ then shouldContinue := false
            succ
        
        let get_values (edmEntitySet:IEdmEntitySet) (shouldContinue) = 
            let value = odataModel.GetQueryable (edmEntitySet)
            if not <| callbacks.Auth(edmEntitySet.ElementType, callbackParameters, value) then 
                shouldContinue := false; null
            else 
                let newVal = callbacks.InterceptMany(edmEntitySet.ElementType, callbackParameters, value) :?> IQueryable
                if newVal <> null 
                then newVal
                else value

        let get_single_result_from_entset edmEntitySet shouldContinue key = 
            let wholeSet = odataModel.GetQueryable (edmEntitySet)
            try
                let singleResult = AstLinqTranslator.select_by_key edmEntitySet.ElementType wholeSet key
                if singleResult <> null then 
                    if auth_item singleResult edmEntitySet.ElementType shouldContinue
                    then 
                        let newVal = callbacks.Intercept(edmEntitySet.ElementType, callbackParameters, singleResult) 
                        if newVal <> null 
                        then newVal
                        else singleResult
                    else null
                else null
            with 
            | exc -> 
                // not found?
                null 

        let get_single_result_from_coll (edmEntityType:IEdmEntityType) (collection:IQueryable) shouldContinue key = 
            try
                let singleResult = AstLinqTranslator.select_by_key edmEntityType collection key
                if singleResult <> null then 
                    if auth_item singleResult edmEntityType shouldContinue
                    then 
                        let newVal = callbacks.Intercept(edmEntityType, callbackParameters, singleResult) 
                        if newVal <> null 
                        then newVal
                        else singleResult
                    else null
                else null
            with 
            | exc -> 
                // not found?
                null 

        /// gets the values from the IQueryable associated with the entityset
        member internal x.GetSetResult (edmEntitySet:IEdmEntitySet, shouldContinue)  =
            get_values edmEntitySet shouldContinue

        /// gets a single value from the IQueryable associated with the entityset using the specified key
        member internal x.GetSingleResult (edmEntitySet:IEdmEntitySet, shouldContinue, key) = 
            get_single_result_from_entset edmEntitySet shouldContinue key

        member internal x.GetSingleResult (edmEntityType:IEdmEntityType, collection, shouldContinue, key) = 
            get_single_result_from_coll edmEntityType collection shouldContinue key


        abstract member Process : op:RequestOperation * 
                                  segment:UriSegment * previous:UriSegment * 
                                  hasMoreSegments:bool * shouldContinue:Ref<bool> * container:obj -> ResponseToSend



