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

namespace Castle.MonoRail
    
    open System
    open System.Web
    open System.Net
    open System.Collections
    open System.Collections.Generic
    open System.Collections.Specialized
    open System.ComponentModel.Composition.Primitives
    open System.ComponentModel.Composition.Hosting
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting
    open Castle.Extensibility.Hosting

    [<AbstractClass>]
    type MrBasedHttpApplication () = 
        inherit HttpApplication()

        [<DefaultValue>] val mutable private _hostingContainer : HostingContainer
        [<DefaultValue>] val mutable private _container : IContainer 
        [<DefaultValue>] val mutable private _canSetContainer : bool

        abstract member Initialize : unit -> unit
        abstract member ConfigureRoutes : router:Router -> unit
        abstract member InitializeContainer : unit -> unit
        abstract member TerminateContainer : unit -> unit
        
        member x.CustomContainer
            with get() = x._container and 
                 set(v) = if x._canSetContainer then x._container <- v else raise(new InvalidOperationException("This can be set only during the Initialize call"))

        default x.Initialize() = ()
        default x.InitializeContainer() = ()
        default x.TerminateContainer() = ()

        member x.Application_Start(sender:obj, args:EventArgs) =
            // todo: move to overridable method
            let bundlesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundles") 
            x._hostingContainer <- new HostingContainer(bundlesPath, new TypeCatalog())
            
            (*
            x._canSetContainer <- true
            x.Initialize()
            x._canSetContainer <- false
            
            if x._container <> null then
                MRComposition.SetCustomContainer x._container
            *)

            MRComposition.SetCustomContainer (ContainerAdapter(x._hostingContainer))

            let router = Router.Instance
            x.ConfigureRoutes(router)
            x.InitializeContainer()

        member x.Application_End(sender:obj, args:EventArgs) = 
            x.TerminateContainer()

       
        
         