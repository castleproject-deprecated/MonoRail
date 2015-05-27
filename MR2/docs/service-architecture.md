# Service Architecture

It's impossible to come up with a sophisticated software where the default behavior pleases everyone and integrates with everything. The usual solution is making the software rely on contracts and having the core code as just a coordination of invocations on the contracts implementation. An user is thus capable of replacing one or more contract implementation.

The challenging is implementing an architecture where the parts are easily replaced, configurable and can rely (depend) on other parts.

MonoRail uses a set of services to handle specific tasks. The framework is responsible for defining the default implementations, instantiate, start and configure them. The services are made available through a combination of lifecycle interfaces and an implementation of `IServiceProvider`.

The most usual solution to this problem is to use an Inversion of Control container. However, things have to be balanced. For MonoRail, an IoC container would introduce dependencies on assemblies and a longer initialization time. In the end, we wouldn't benefit from all IoC container features, so it could be considered too much for our problem.

Instead, we combined what is already on the .NET library and some creative solutions.

Basically we create two levels of services registries, per application and per request, and a simple interfaces that defines lifecycles that services optionally implement. This allows the service to start its work when it is supposed to and to gather reference to other services.

## How It Works

When the web application is started, the ASP.Net modules are initialized. MonoRail has an `EngineContextModule` which is in charge of:

* Reading the configuration
* Initializing the services
* Subscribing to ASP.Net application and request level events
* Create a request context (which we'll not cover here)

Services implementation can be defined in the configuration section. After reading the configuration section MonoRail checks for missing definition and register the missing services using the default implementation.

After that it instantiates every service and runs the lifecycle. If everything went well, the framework is properly initialized. All services are registered in the application level container, which happens to be implemented by the `EngineContextModule` class. We call this the parent container.

When a request starts, MonoRail creates a `DefaultRailsEngineContext` instance, which also is a container for services. MonoRail sets the parent container on it. This allow the user to override services per request, and resolution of services in the parent.

* The configuration is read into the `MonoRailConfiguration`. It's also registered as a service
* The services collected are instantiated and registered
* The lifecycle is executed

## Lifecycle Interfaces

A service might implement a few interfaces to expose to MonoRail that it behaves in a specific way or that it needs something from the framework.

* `ISupportInitialize` (from System.ComponentModel)

This interface can be implemented if a service wants performs some initialization.

* `IInitializable` (from Castle.Core)

This interface can be implemented if a service wants performs some initialization.

* `IServiceEnabledComponent` (from Castle.Core)

This interface can be implemented if a service uses other services.
`IServiceEnabledComponent` is the first one to be invoked. This gives a chance to gather all services references it wants. Then the initialization interface's methods are invoked.

For services that use the `IServiceEnabledComponent` lifecycle, the implementor should keep in mind that the initialization lifecycle has not run for all services, so it might not be safe to use other services as they might not be properly initialized at the moment.

The order of service registration and instantiation is not guaranted. So the implementor should not make any assumption regarding it.

## Registering Services

You have to use the monorail configuration node to override or add services to MonoRail.

## Built-In Services

The following is a list of build-in services and their roles. You can refer to this list to learn more about MonoRail inner workings or to use them when developing extensions and new services.

* `MonoRailConfiguration`: Exposes the MonoRail configuration
* `ExtensionManager`: Manages registered extensions dispatching events from Asp.Net infrastructure and from MonoRail services
* `IViewSourceLoader`: Sits in front of the file system and from assembly resources. It is used by view engines to obtain view streams
* `IViewEngine`: Process view templates
* `IScaffoldingSupport`: Adds scaffold support to a controller
* `IControllerFactory`: Creates the controller instances
* `IViewComponentFactory`: Manages registered ViewComponents and creates their instances
* `IFilterFactory`: Manages registered filters and creates their instances
* `IResourceFactory`: Create resources
* `IEmailSender`: Sends e-mail
* `IEmailTemplateService`: Process e-mail templates using the MonoRail infrastructure
* `IControllerDescriptorProvider`: Inspects Controller types building a descriptor of what has been defined using attributes
* `IResourceDescriptorProvider`: Creates descriptors for resources declared on controllers
* `IRescueDescriptorProvider`: Creates descriptors for rescues declared on controllers
* `ILayoutDescriptorProvider`: Creates descriptors for layouts declared on controllers
* `IHelperDescriptorProvider`: Creates descriptors for helpers declared on controllers
* `IFilterDescriptorProvider`: Creates descriptors for filters declared on controllers
* `IControllerTree`: Manages a binary tree of controllers registered
* `ICacheProvider`: Manages the cache