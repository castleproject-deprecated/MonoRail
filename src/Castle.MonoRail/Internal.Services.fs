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

    [<Export(typeof<IServiceRegistry>)>]
    type ServiceRegistry() = 
        interface IServiceRegistry with 
            member x.Get ( service:'T ) : 'T = 
                Unchecked.defaultof<_>


    type EnvironmentServicesBridge() =
        
        [<Export>]
        member x.HttpContext : HttpContextBase = 
             HttpContextWrapper(HttpContext.Current) :> HttpContextBase

        [<Export>]
        member x.HttpRequest : HttpRequestBase = 
             HttpRequestWrapper(HttpContext.Current.Request) :> HttpRequestBase

        [<Export>]
        member x.HttpResponse : HttpResponseBase = 
             HttpResponseWrapper(HttpContext.Current.Response) :> HttpResponseBase

        [<Export>]
        member x.HttpServer : HttpServerUtilityBase = 
             HttpServerUtilityWrapper(HttpContext.Current.Server) :> HttpServerUtilityBase

        [<Export>]
        member x.RouteMatch : RouteMatch = 
             HttpContext.Current.Items.[Constants.MR_Routing_Key] :?> RouteMatch
