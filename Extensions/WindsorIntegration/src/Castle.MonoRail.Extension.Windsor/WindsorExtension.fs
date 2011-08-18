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

namespace Castle.MonoRail.Extension.Windsor

    open System
    open System.Collections.Generic
    open System.Web
    open System.Threading
    open System.ComponentModel.Composition
    open Castle.MicroKernel.Facilities
    open Castle.Windsor
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Castle.MonoRail.Hosting.Mvc.Typed

    module ContainerAccessorUtil = 
        begin

            let internal InternalObtainContainer (accessor:IContainerAccessor) : IWindsorContainer =     
                if obj.ReferenceEquals(accessor, null) then
                    raise (MonoRailException("You must extend the HttpApplication in your web project " +
                                             "and implement the IContainerAccessor to properly expose your container instance"))
                else 
                    let container = accessor.Container;

                    if obj.ReferenceEquals(container, null) then
                        raise(MonoRailException("The container seems to be unavailable in " +
                                                "your HttpApplication subclass"))
                    else 
                        container
            
            let ObtainContainer() : IWindsorContainer = 
                let accessor =  HttpContext.Current.ApplicationInstance |> box :?> IContainerAccessor
                InternalObtainContainer accessor
        end

    module WindsorUtil = 
        begin
            let internal BuildArguments (context:HttpContextBase) : Dictionary<string, obj> = 
                let args = Dictionary<string, obj>()
                args.Add("context", context)
                args.Add("principal", lazy (context.User))

                args
        end

    [<ControllerProviderExport(900000)>] 
    type WindsorControllerProvider() =
        inherit ControllerProvider()

        let nullPrototype = Unchecked.defaultof<ControllerPrototype>
        // let mutable _initialized = ref 0
        let _containerInstance = lazy ( ContainerAccessorUtil.ObtainContainer() ) 

        (*
        let initialize() = 
            ()

        let ensure_initialized instance = 
            if Interlocked.CompareExchange(_initialized, 1, 0) = 0 then
                initialize()
        *)

        let mutable _desc_builder = Unchecked.defaultof<ControllerDescriptorBuilder>

        let normalize_name (cname:string) =
            if cname.EndsWith("Component", StringComparison.OrdinalIgnoreCase) then
                cname
            else
                cname + "Controller" 

        [<Import>]
        member this.ControllerDescriptorBuilder
            with get() = _desc_builder and set(v) = _desc_builder <- v
            
        override this.Create(data:RouteMatch, context:HttpContextBase) = 
            // ensure_initialized(this)

            let _, area = data.RouteParams.TryGetValue "area"
            let hasCont, controller = data.RouteParams.TryGetValue "controller"
            
            if hasCont then 
                let key = (sprintf "%s\\%s" area (normalize_name controller)).ToLowerInvariant()
                let container = _containerInstance.Force()
                if container.Kernel.HasComponent(key) then
                    
                    let instance = container.Resolve<obj>(key, WindsorUtil.BuildArguments(context))
                    let cType = instance.GetType()
                    let desc = _desc_builder.Build(cType)
                    upcast TypedControllerPrototype(desc, instance) 
                else 
                    nullPrototype
            else 
                nullPrototype


    [<Export(typeof<IFilterActivator>)>]
    [<ExportMetadata("Order", 90000)>]
    type WindsorFilterActivator() =
        let _containerInstance = lazy ( ContainerAccessorUtil.ObtainContainer() ) 

        interface IFilterActivator with
            member this.ActivateBeforeActionFilter(filter:Type, context:HttpContextBase) : IBeforeActionFilter =
                let container = _containerInstance.Force()

                if container.Kernel.HasComponent(filter) then
                    container.Resolve(filter, WindsorUtil.BuildArguments(context)) :?> IBeforeActionFilter
                else
                    Unchecked.defaultof<IBeforeActionFilter>
            
            member this.ActivateAfterActionFilter(filter:Type, context:HttpContextBase) : IAfterActionFilter =
                let container = _containerInstance.Force()

                if container.Kernel.HasComponent(filter) then
                    container.Resolve(filter, WindsorUtil.BuildArguments(context)) :?> IAfterActionFilter
                else
                    Unchecked.defaultof<IAfterActionFilter>

            member this.ActivateExceptionFilter(filter:Type, context:HttpContextBase) : IExceptionFilter =
                let container = _containerInstance.Force()

                if container.Kernel.HasComponent(filter) then
                    container.Resolve(filter, WindsorUtil.BuildArguments(context)) :?> IExceptionFilter
                else
                    Unchecked.defaultof<IExceptionFilter>


    