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
    open System.Collections.Specialized
    open Castle.MonoRail.Mvc.ViewEngines

    [<Interface>]
    type public IServiceRegistry =
        abstract member ViewEngines : IViewEngine seq with get
        abstract member ViewFolderLayout : IViewFolderLayout
        abstract member Get : service:'T -> 'T
        abstract member GetAll : service:'T -> 'T seq


    // very early incarnation 
    type PropertyBag() = 
        let _bag = HybridDictionary(true)

        member x.Item
            with get(name:string) = _bag.[name] and set (name:string) v = _bag.[name] <- v


    /// <summary>
    /// Optional base class for controllers
    /// </summary>
    [<AbstractClass>]
    type Controller() = 
        let mutable _req = Unchecked.defaultof<HttpRequestBase>
        
        member x.Request 
            with get() = _req and set v = _req <- v


