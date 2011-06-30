//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail
    
    open System
    open System.Web
    open System.Collections
    open System.Collections.Generic
    open System.Collections.Specialized
    open Castle.MonoRail.Routing
    open System.Dynamic


    [<Interface>]
    type IModuleStarter = 
        interface 
            abstract member Initialize : unit -> unit
        end


    [<Interface>]
    type IModelAccessor<'a> = 
        abstract member Model : 'a


    // very early incarnation 
    type PropertyBag<'TModel when 'TModel : null>() = 
        inherit DynamicObject()
        let mutable _model : 'TModel = null
        let _props = Dictionary<string,obj>()

        member private x.Props = _props

        member private this.GetValue name = 
            let res, value = _props.TryGetValue name
            if res then 
                value
            else 
                null

        member private this.SetValue (name,value) =
            _props.[name] <- value

        override this.TryGetMember (binder:GetMemberBinder, result:obj byref) =     
            let r, tmp = _props.TryGetValue binder.Name
            result <- tmp
            r
    
        override this.TrySetMember(binder:SetMemberBinder, value:obj) =        
            this.SetValue(binder.Name, value)
            true
    
        override this.GetDynamicMemberNames() =
            upcast _props.Keys
    
        interface IDictionary<string,obj> with 
            member x.Add (key, value) = 
                _props.Add (key, value)

            member x.Remove key = 
                _props.Remove key

            member x.ContainsKey key = 
                _props.ContainsKey key

            member x.TryGetValue (key, result:obj byref) =
                _props.TryGetValue (key, ref result)

            member x.Item 
                with get(key) = _props.Item(key) 
                and  set key v = _props.[key] <- v

            member x.Keys = upcast _props.Keys
            member x.Values = upcast _props.Values

        interface ICollection<KeyValuePair<string,obj>> with 
            member x.Add (pair) = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).Add pair
            member x.IsReadOnly = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).IsReadOnly
            member x.Clear() = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).Clear()
            member x.Contains(item) = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).Contains(item)
            member x.Remove(item) = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).Remove(item)
            member x.Count = _props.Count
            member x.CopyTo (array, index) = 
                (_props |> box :?> ICollection<KeyValuePair<string,obj>>).CopyTo(array, index)

        interface IEnumerable<KeyValuePair<string,obj>> with 
            member x.GetEnumerator() =
                (_props |> box :?> IEnumerable<KeyValuePair<string,obj>>).GetEnumerator()

        interface IEnumerable with 
            member x.GetEnumerator() =
                (_props |> box :?> IEnumerable).GetEnumerator()

        member x.Model 
            with get() = _model and set v = _model <- v

        member x.Item 
            with get(key) = _props.Item(key) 
            and  set key v = _props.[key] <- v

        interface IModelAccessor<'TModel> with 
            member x.Model = _model



    type PropertyBag() = 
        inherit PropertyBag<obj>()



    type Model<'TModel>(model:'TModel) = 
        let _model = model
        member x.Value = _model
        member x.IsValid = true
        // validation stuff here


    (*
    [<AbstractClass>]
    type Controller() = 
        let mutable _req = Unchecked.defaultof<HttpRequestBase>
        
        member x.Request 
            with get() = _req and set v = _req <- v
    *)

    [<AbstractClass>]
    type GeneratedUrlsBase() = 
        static let mutable _vpath = HttpContext.Current.Request.ApplicationPath
        static let mutable _router = Router.Instance

        static member VirtualPath 
            with get() = _vpath and set v = _vpath <- v

        static member CurrentRouter
            with get() = _router and set v = _router <- v

    
    type UrlParameters(controller:string, action:string, [<ParamArray>] entries:KeyValuePair<string,string>[]) =
         inherit Dictionary<string,string>()
         do
            base.Add("controller",  controller)
            base.Add("action",  action)

            for pair in entries do
                base.Add(pair.Key, pair.Value)

    [<AbstractClass>]
    type TargetUrl() = 
        abstract member Generate : parameters:IDictionary<string,string> -> string


    and RouteBasedTargetUrl(vpath:string, route:Route, parameters:IDictionary<string,string>) = 
        inherit TargetUrl()
        let _vpath = vpath
        let _route = route
        let _fixedParams = parameters

        let merge (newParams:IDictionary<string,string>) = 
            if newParams != null then
                let dict = Dictionary<string,string>(_fixedParams)
                for pair in newParams do
                    dict.[pair.Key] <- pair.Value
                dict :> IDictionary<string,string>
            else
                _fixedParams 

        override x.Generate parameters = 
            _route.Generate (vpath, (merge parameters))

        override x.ToString() =
            _route.Generate (vpath, _fixedParams)

            
    type MimeType = 
        | Html = 1
        | Xml = 2
        | JSon = 3
        | Js = 4
        | Atom = 5
        | Rss = 6
        | FormUrlEncoded = 7
        | Unknown = -1