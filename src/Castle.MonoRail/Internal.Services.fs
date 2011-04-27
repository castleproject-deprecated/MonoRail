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
    open Castle.MonoRail.Mvc.ViewEngines

    [<Export(typeof<IServiceRegistry>)>]
    type ServiceRegistry() =
        let mutable _viewEngines = System.Linq.Enumerable.Empty<IViewEngine>()

        [<ImportMany(AllowRecomposition=true)>]
        member x.ViewEngines
            with set v = _viewEngines <- v
         
        interface IServiceRegistry with 
            
            member x.ViewEngines = _viewEngines

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
