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

[<System.Runtime.CompilerServices.ExtensionAttribute>]
module public ExtensionMethods = 

    open System
    open Castle.MonoRail.Hosting.Mvc
    open System.Runtime.CompilerServices

    [<ExtensionAttribute>]
    [<CompiledName("Match")>]
    let MatchExt1(router:Router, path:string) = 
        router.Match(path, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Match")>]
    let MatchExt2(router:Router, path:string, name:string) = 
        router.Match(path, name, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Match")>]
    let MatchExt3(router:Router, path:string, config:Action<RouteConfig>) = 
        router.Match(path, config, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Match")>]
    let MatchExt4(router:Router, path:string, name:string, config:Action<RouteConfig>) = 
        router.Match(path, name, config, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Resource")>]
    let ResourceExt1(router:Router, name:string) = 
        router.Resource(name, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Resources")>]
    let ResourcesExt1(router:Router, name:string) = 
        router.Resources(name, MonoRailHandlerMediator())

    [<ExtensionAttribute>]
    [<CompiledName("Resources")>]
    let ResourcesExt2(router:Router, name:string, identifier:string) = 
        router.Resources(name, identifier, MonoRailHandlerMediator())
    
