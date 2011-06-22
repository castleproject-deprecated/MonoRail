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

module Internal

    open System.ComponentModel.Composition
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Serialization
    open Castle.MonoRail.ViewEngines
    open Castle.MonoRail.Hosting.Mvc.Typed
    open Castle.MonoRail.ViewComponents

    [<Export(typeof<IServiceRegistry>)>]
    type ServiceRegistry() =
        let mutable _viewEngines = System.Linq.Enumerable.Empty<IViewEngine>()
        let mutable _viewFolderLayout : IViewFolderLayout = Unchecked.defaultof<_>
        let mutable _viewRendererService : ViewRendererService = Unchecked.defaultof<_>
        let mutable _modelSerializerResolver : ModelSerializerResolver = Unchecked.defaultof<_>
        let mutable _modelHypertextProcessorResolver : ModelHypertextProcessorResolver = Unchecked.defaultof<_>
        let mutable _contentNegotiator : ContentNegotiator = Unchecked.defaultof<_>
        let mutable _vcExecutor : ViewComponentExecutor = Unchecked.defaultof<_>

        [<ImportMany(AllowRecomposition=true)>]
        member x.ViewEngines
            with set v = _viewEngines <- v

        [<Import(AllowRecomposition=true)>]
        member x.ViewFolderLayout
            with set v = _viewFolderLayout <- v

        [<Import(AllowRecomposition=true)>]
        member x.ViewRendererService
            with set v = _viewRendererService <- v

        [<Import(AllowRecomposition=true)>]
        member x.ModelSerializerResolver
            with set v = _modelSerializerResolver <- v

        [<Import(AllowRecomposition=true)>]
        member x.ModelHypertextProcessorResolver
            with set v = _modelHypertextProcessorResolver <- v

        [<Import(AllowRecomposition=true)>]
        member x.ContentNegotiator
            with set v = _contentNegotiator <- v

        [<Import(AllowRecomposition=true)>]
        member x.ViewComponentExecutor
            with set v = _vcExecutor <- v

        interface IServiceRegistry with 
            member x.ViewEngines = _viewEngines
            member x.ViewFolderLayout = _viewFolderLayout
            member x.ViewRendererService = _viewRendererService
            member x.ModelSerializerResolver = _modelSerializerResolver
            member x.ModelHypertextProcessorResolver = _modelHypertextProcessorResolver
            member x.ContentNegotiator = _contentNegotiator
            member x.ViewComponentExecutor = _vcExecutor

            member x.Get ( service:'T ) : 'T = 
                Unchecked.defaultof<_>
            member x.GetAll ( service:'T ) : 'T seq = 
                Unchecked.defaultof<_>


    [<PartMetadata("Scope", ComponentScope.Request)>]
    type EnvironmentServicesBridge() =
        
        [<Export>]
        member x.HttpContext : HttpContextBase = 
             upcast HttpContextWrapper(HttpContext.Current) 

        [<Export>]
        member x.HttpRequest : HttpRequestBase = 
             upcast HttpRequestWrapper(HttpContext.Current.Request) 

        [<Export>]
        member x.HttpResponse : HttpResponseBase = 
             upcast HttpResponseWrapper(HttpContext.Current.Response) 

        [<Export>]
        member x.HttpServer : HttpServerUtilityBase = 
             upcast HttpServerUtilityWrapper(HttpContext.Current.Server) 

        [<Export>]
        member x.RouteMatch : RouteMatch = 
             HttpContext.Current.Items.[Constants.MR_Routing_Key] :?> RouteMatch



