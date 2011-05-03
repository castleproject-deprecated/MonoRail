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

    and RouteBasedTargetUrl(route:Route) = 
        inherit TargetUrl()
        override x.Generate parameters = 
            // route.Generate
            ""



