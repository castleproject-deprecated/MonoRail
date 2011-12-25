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

namespace Castle.MonoRail.ViewEngines

    open System
    open System.IO
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.Linq
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.Resource

    // we need to make sure this interface allows for recursive view engines
    // ie. view engines that would allow for application of layouts recursively with no change needed in its api

    // todo: refactor so it doesn't mention action/controller/area

    type ViewRequest() = 
        let mutable _viewName : string = null
        let mutable _outerViewName : string = null
        let mutable _groupFolder : string = null
        let mutable _viewFolder : string = null
        let mutable _viewLocations : string seq = null
        let mutable _layoutLocations : string seq = null
        let mutable _defaultName : string = null
        let mutable _vpath : string = String.Empty
        let mutable _processed = false

        member this.GroupFolder
            with get() = _groupFolder and set v = _groupFolder <- v

        member this.ViewFolder
            with get() = _viewFolder and set v = _viewFolder <- v
        
        member this.ViewName
            with get() = _viewName and set v = _viewName <- v
        
        member this.DefaultName 
            with get() = _defaultName and set v = _defaultName <- v
        
        member this.OuterViewName 
            with get() = _outerViewName and set v = _outerViewName <- v

        member this.ViewLocations with get() = _viewLocations and set v = _viewLocations <- v
        member this.LayoutLocations with get() = _layoutLocations and set v = _layoutLocations <- v

        member x.CreatePartialRequest(partialName:string) = 
            ViewRequest( GroupFolder = x.GroupFolder, ViewFolder = x.ViewFolder, ViewName = partialName ) 

        member internal x.SetProcessed() = 
            _processed <- true
        member internal x.WasProcessed = _processed


    type ViewEngineResult(view:IView, engine:IViewEngine) = 
        let _view = view
        let _engine = engine

        new () = 
            ViewEngineResult(Unchecked.defaultof<_>, Unchecked.defaultof<_>)

        member x.View = _view
        member x.Engine = _engine
        member x.IsSuccessful = _view != null
    

    and [<Interface>] 
        public IViewEngine =
            abstract member HasView : viewLocations:string seq -> bool
            abstract member ResolveView : viewLocations:string seq * layoutLocations:string seq -> ViewEngineResult


    and [<Interface>] 
        public IView =
            abstract member Process : writer:TextWriter * ctx:ViewContext -> unit

    and 
        public ViewContext(httpctx:HttpContextBase, bag:IDictionary<string,obj>, model, viewRequest:ViewRequest) = 
            let _httpctx = httpctx
            let _model = model
            let mutable _writer = lazy( _httpctx.Response.Output )

            member x.HttpContext = _httpctx
            member x.Model = _model
            member x.Bag = bag
            member x.Writer  with get() = _writer.Force() and set v = _writer <- lazy( v )
            member x.ViewRequest = viewRequest


    [<AbstractClass>]
    type BaseViewEngine() = 
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()

        let rec provider_sel (enumerator:IEnumerator<ResourceProvider>) paths : string seq * ResourceProvider = 
            if (enumerator.MoveNext()) then
                let provider = enumerator.Current
                let existing_view = 
                    paths |> Seq.filter (fun (v) -> provider.Exists(v))
                
                if existing_view = null then 
                    provider_sel enumerator paths
                else 
                    existing_view, provider
            else
                null, Unchecked.defaultof<_>

        and find_provider paths = 
            use enumerator = _resProviders.GetEnumerator()
            let paths, provider = provider_sel enumerator paths 
            if Seq.isEmpty paths then
                null, Unchecked.defaultof<_>
            else 
                paths, provider
        
        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders
            with get() = _resProviders and set v = _resProviders <- v

        member x.FindProvider(paths) = 
            find_provider paths

        abstract member ResolveView : viewLocations:string seq * layoutLocations:string seq -> ViewEngineResult
        abstract member HasView : viewLocations:string seq -> bool

        interface IViewEngine with 
            member this.ResolveView (viewLocations, layoutLocations) = 
                this.ResolveView(viewLocations, layoutLocations)
            
            member this.HasView (viewLocations) = 
                this.HasView(viewLocations)




