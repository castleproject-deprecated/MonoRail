namespace Castle.MonoRail.Extension.Windsor

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


    (* 
    type MonoRailFacility() = 
        inherit AbstractFacility() 

        override x.Init() = 
            // Kernel.ComponentModelCreated += OnComponentModelCreated;
            ()
    *)


    [<ControllerProviderExport(900000)>] 
    type WindsorControllerProvider() =
        inherit ControllerProvider()

        let nullPrototype = Unchecked.defaultof<ControllerPrototype>
        let mutable _initialized = ref 0
        let _containerInstance = lazy ( ContainerAccessorUtil.ObtainContainer() ) 

        (*
        let initialize() = 
            ()

        let ensure_initialized instance = 
            if Interlocked.CompareExchange(_initialized, 1, 0) = 0 then
                initialize()
        *)

        let mutable _desc_builder = Unchecked.defaultof<ControllerDescriptorBuilder>

        [<Import>]
        member this.ControllerDescriptorBuilder
            with get() = _desc_builder and set(v) = _desc_builder <- v
            
        override this.Create(data:RouteMatch, context:HttpContextBase) = 
            // ensure_initialized(this)

            let _, area = data.RouteParams.TryGetValue "area"
            let hasCont, controller = data.RouteParams.TryGetValue "controller"
            
            if hasCont then 
                let key = (sprintf "%s\\%sController" area controller).ToLowerInvariant()
                let container = _containerInstance.Force()
                if container.Kernel.HasComponent(key) then
                    let instance = container.Resolve(key)
                    let cType = instance.GetType()
                    let desc = _desc_builder.Build(cType)
                    upcast TypedControllerPrototype(desc, instance) 
                else 
                    nullPrototype
            else 
                nullPrototype

