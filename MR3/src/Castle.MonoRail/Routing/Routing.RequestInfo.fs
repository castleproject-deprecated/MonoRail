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

namespace Castle.MonoRail.Routing

    open System
    open System.Collections.Generic
    open System.Threading
    open System.Web
    open Internal
    open Helpers

    type RequestInfo (path:string, protocol:string, domain:string, port:int, rootPath:string) = 
        let mutable _pathStartIndex = 0

        do 
            if rootPath <> null && rootPath.Length <> 1 then
                _pathStartIndex <- rootPath.Length

        new (path, uri:Uri, vpath) = 
            RequestInfo(path, uri.Scheme, uri.Host, uri.Port, vpath)
        new (request:HttpRequestBase) =
            RequestInfo(request.Path, request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath)
        new (request:HttpRequest) =
            RequestInfo(request.Path, request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath)

        // should be renamed to virtual path?
        member this.RootPath = rootPath 
        member this.Path = path
        member this.Protocol = protocol
        member this.Domain = domain
        member this.BaseUri = (Uri((sprintf "%s://%s:%d%s" protocol domain port rootPath)))


        member this.PathStartIndex = _pathStartIndex
        member this.PathStartIndex with set v = _pathStartIndex <- v

