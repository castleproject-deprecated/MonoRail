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

namespace Castle.MonoRail.Hosting

    open System.Web


    [<Interface>]
    type public IComposableHandler =
        abstract member ProcessRequest : request:HttpContextBase -> unit



namespace Castle.MonoRail

    
    [<Interface>]
    type public IServiceRegistry =
        abstract member Get : service:'T -> 'T


namespace Castle.MonoRail.Hosting.Mvc

    open System.Web
    open System.Collections.Generic
    open Castle.MonoRail.Routing

    type ControllerPrototype(controller:obj) =
        let _meta = Dictionary<string,obj>()
        let _instance = controller
        do 
            Assertions.ArgNotNull (controller, "controller")
        
        member this.Metadata 
            with get() = _meta :> IDictionary<string,obj>
        
        member this.Instance
            with get() = _instance

    [<AbstractClass>]
    type ControllerProvider() = 
        abstract member Create : data:RouteMatch * context:HttpContextBase -> ControllerPrototype

    [<AbstractClass>]
    type ControllerExecutor() = 
        abstract member Execute : controller:ControllerPrototype * route_data:RouteMatch * context:HttpContextBase -> unit

    [<AbstractClass>]
    type ControllerExecutorProvider() = 
        abstract member Create : prototype:ControllerPrototype * data:RouteMatch * context:HttpContextBase -> ControllerExecutor


namespace Castle.MonoRail.Hosting.Mvc.ViewEngines

    open System
    open System.IO

    type ViewRequest(viewName:string, layout:string, area:string) = 
        let _viewName = viewName
        let _layout = layout
        let _area = area
        member this.ViewName 
            with get() = _viewName
        member this.LayoutName 
            with get() = _layout
        member this.AreaName
            with get() = _area

    type ViewEngineResult(view:IView, engine:IViewEngine) = 
        let _view = view
        let _engine = engine
        member this.View = _view
            // with get() = _view
        member this.Engine = _engine
            // with get() = _engine
    
    and [<Interface>] 
        public IViewEngine =
            abstract member ResolveView : req:ViewRequest -> ViewEngineResult

    and [<Interface>] 
        public IView =
            abstract member Process : (* some param here *) writer:TextWriter -> unit
             

namespace Castle.MonoRail.Extensibility

    open System
    open System.ComponentModel.Composition
    open Castle.MonoRail.Hosting.Mvc

    type public ComponentScope =
    | Application = 0
    | Request = 1
    // PartMetadata is used to put components in the request of app scope, ie.
    // PartMetadata("Scope", ComponentScope.Application)

    [<Interface>]
    type public IComponentOrder = 
        abstract member Order : int

    [<MetadataAttribute>]
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple=false)>]
    type public ControllerProviderExportAttribute(order:int) =
        inherit ExportAttribute(typeof<ControllerProvider>)
        let _order = order
        
        member x.Order 
            with get() = _order

    [<MetadataAttribute>]
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple=false)>]
    type public ControllerExecutorProviderExportAttribute(order:int) =
        inherit ExportAttribute(typeof<ControllerExecutorProvider>)
        let _order = order
        
        member x.Order 
            with get() = _order
