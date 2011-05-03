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
    
    open System.Web
    open System.Collections.Generic
    open System.Collections.Specialized
    open Castle.MonoRail.Mvc.ViewEngines
    open Castle.MonoRail.Routing
    open Helpers

    [<Interface>]
    type IModelAccessor<'a> = 
        abstract member Model : 'a


    [<Interface>]
    type public IServiceRegistry =
        abstract member ViewEngines : IViewEngine seq with get
        abstract member ViewFolderLayout : IViewFolderLayout
        abstract member ViewRendererService : ViewRendererService
        abstract member Get : service:'T -> 'T
        abstract member GetAll : service:'T -> 'T seq


    // very early incarnation 
    type PropertyBag() = 
        let mutable _model : obj = null
        let _bag = lazy HybridDictionary(true)

        member x.Item
            with get(name:string) = _bag.Force().[name] and set (name:string) v = _bag.Force().[name] <- v

        member x.Model 
            with get() = _model and set v = _model <- v


    type Model<'TModel>(model:'TModel) = 
        let _model = model
        member x.Value = _model
        member x.IsValid = false
        // validation stuff here


    (*
    [<AbstractClass>]
    type Controller() = 
        let mutable _req = Unchecked.defaultof<HttpRequestBase>
        
        member x.Request 
            with get() = _req and set v = _req <- v
    *)

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
            
    type MimeType = 
        | Xhtml = 1
        | Xml = 2
        | JSon = 3
        | Js = 4
        | Atom = 5
        | Rss = 6


