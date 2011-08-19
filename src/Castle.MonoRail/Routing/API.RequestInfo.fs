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

namespace Castle.MonoRail.Routing

    open System
    open System.Collections.Generic
    open System.Threading
    open System.Web
    open Internal
    open Helpers

    [<Interface>]
    type IRequestInfo = 
        abstract RootPath : string
        abstract Path : string
        abstract Protocol : string
        abstract HttpMethod : string
        abstract Domain : string
        abstract PathStartIndex : int with get, set

    type RequestInfoAdapter(path:string, protocol:string, httpMethod:string, domain:string, rootPath:string) = 
        let mutable _pathStartIndex = 0

        do 
            if rootPath != null then
                _pathStartIndex <- rootPath.Length

        new (request:HttpRequestBase) =
            RequestInfoAdapter(request.Path, request.Url.Scheme, request.HttpMethod, request.Url.Host, request.ApplicationPath)
        new (request:HttpRequest) =
            RequestInfoAdapter(request.Path, request.Url.Scheme, request.HttpMethod, request.Url.Host, request.ApplicationPath)

        interface IRequestInfo with
            member this.RootPath = rootPath
            member this.Path = path
            member this.Protocol = protocol
            member this.HttpMethod = httpMethod
            member this.Domain = domain
            member this.PathStartIndex = _pathStartIndex
            member x.PathStartIndex
                with set v = _pathStartIndex <- v