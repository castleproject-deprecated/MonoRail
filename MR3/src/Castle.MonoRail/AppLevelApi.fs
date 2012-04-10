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
    open System.ComponentModel.Composition
    open System.ComponentModel.Composition.Primitives
    open System.ComponentModel.Composition.Hosting
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting
    open Castle.MonoRail.Hosting.Container
    open Castle.Extensibility
    open Castle.Extensibility.Hosting


    [<Interface; AllowNullLiteral>]
    // [<InheritedExport>]
    type IMonoRailConfigurer = 
        abstract member Configure : services:IServiceRegistry -> unit  


    [<Export(typeof<IModuleStarter>)>]
    type internal DefaultStarter() = 
        let _registry : Ref<IServiceRegistry> = ref null
        let _app : Ref<HttpApplication> = ref null

        let configure (t:Type) = 
            let instance = Activator.CreateInstance(t) :?> IMonoRailConfigurer
            instance.Configure(!_registry)

        [<Import>]
        member x.Registry with get() = !_registry and set v = _registry := v

        [<Import>]
        member x.HttpApp  with get() = !_app and set v = _app := v

        interface IModuleStarter with 

            member x.Initialize(ctx) = 
                let bindCtx = ctx.GetService<IBindingContext>()
                let configurers = 
                    let types = 
                        if bindCtx <> null 
                        then bindCtx.GetAllTypes()
                        else
                            if x.HttpApp <> null then
                                let baseApp = x.HttpApp.GetType()
                                if baseApp.BaseType = typeof<obj> 
                                then baseApp.Assembly.GetExportedTypes() |> Array.toSeq
                                else baseApp.BaseType.Assembly.GetExportedTypes() |> Array.toSeq 
                            else Seq.empty
                    types |> Seq.filter( (fun t -> not t.IsAbstract && not t.IsInterface && typeof<IMonoRailConfigurer>.IsAssignableFrom(t) ) )
                configurers |> Seq.iter configure

            member x.Terminate() = 
                ()


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
        abstract member Configure : services:IServiceRegistry -> unit
        
        
        member x.CustomContainer
            with get() = x._container and 
                 set(v) = if x._canSetContainer then x._container <- v else raise(new InvalidOperationException("This can be set only during the Initialize call"))
        

        default x.Initialize() = ()
        default x.InitializeContainer() = ()
        default x.TerminateContainer() = ()
        default x.Configure(services) = ()

        member x.Application_Start(sender:obj, args:EventArgs) =

            x._canSetContainer <- true
            x.Initialize()
            x._canSetContainer <- false
            
            if x._container <> null then
                // the user set up a custom container
                MRComposition.SetCustomContainer x._container
            (*
            else
                // let's go with the default
                let bundlesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundles") 
                let binPath = AppDomain.CurrentDomain.RelativeSearchPath
                let defaultMrCatalog = (new Container(binPath)).DefaultMrCatalog
                x._hostingContainer <- new HostingContainer(bundlesPath, defaultMrCatalog)
                MRComposition.SetCustomContainer (ContainerAdapter(x._hostingContainer))
            *)

            let router = Router.Instance
            x.ConfigureRoutes(router)
            x.InitializeContainer()

        member x.Application_End(sender:obj, args:EventArgs) = 
            x.TerminateContainer()

        //interface IMonoRailConfigurer with
        //    member x.Configure(services) = x.Configure(services)
                
        
       
 
         