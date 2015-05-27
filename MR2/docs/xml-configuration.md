# XML Configuration

This topic provides detailed information on configuring MonoRail using XML in the web.config file. Using this approach assumes that ASP.NET has already been configured as per the [Configuration](configuration.md). An alternative approach to configuration that can also be used in conjunction with the XML configuration detailed in this topic is [programmatic configuration](programmatic-configuration.md).

## Adding the Configuration Section Handler

To configure MonoRail using XML in the web.config file you must first declare the MonoRail configuration handler in the `<configSections>` node of your application's web.config.

The following snipped demonstrates registration of the MonoRail configuration handler.

```xml
<configuration>
  <configSections>
    <section
      name="monorail"
      type="Castle.MonoRail.Framework.Configuration.MonoRailSectionHandler, Castle.MonoRail.Framework" />
    ...
  </configSections>
  ...
</configuration>
```

This section handler specifies that configuration for MonoRail should be taken from a an XML configuration element named `<monorail>` in the application's web.config.

**Section Handlers:** For further information on registering section handlers in your web.config file please see the [Microsoft documentation on the subject](http://msdn.microsoft.com/en-us/library/w7w4sb0w.aspx).

Once the handler is registered the `<monorail>` element should be added under the root `<configuration>` node of the application's web.config. For a full schema that details all available elements and attributes supported by the `<monorail>` element see the [XML Configuration Schema](xml-configuration-schema.md). In addition you may find a [sample of a complete XML](xml-configuration-sample.md) informative.

**XML Intellisense:** Intellisense can be exposed in Visual Studio by registering the XSD into the Schemas folder and adding the `xmlns="urn:castle-monorail-configuration-2.0"` attribute to the `<monorail>` node in your configuration.

The rest of this topic describes the `<monorail>` element and its associated attributes and child elements.

## The `<monorail>` Element

The `<monorail>` element is the root element of the MonoRail configuration and contains all of the attributes and child elements that control the runtime behavior of MonoRail.

The `<monorail>` element supports the following attributes:

Attribute | Description
----------|------------
smtpHost | The address of the host that exposes an SMTP server with which the smtpService will communicate if configured.
smtpPort | The port to use on the SMTP server in the event that a non-standard port is exposed.
smtpUsername | The username to use when authenticating with the SMTP server if authentication is required.
smtpPassword | The password that corresponds with the username for the SMTP server.
smtpUseSsl | Whether or not an SSL connection should be used with communicating with the SMTP server.

```xml
<monorail xmlns="urn:castle-monorail-configuration-2.0"
  smtpHost="smtp.host.com" smtpPort="443"
  smtpUsername="myuser" smtpPassword="mypass"
  smtpUseSsl="true">
  ...
</monorail>
```

In addition to these attributes, the `<monorail>` node can contain one of each of the following child elements.

Element | Description
--------|------------
controllers | Registers assemblies from which MonoRail should load controller types.
extensions | Registers extensions with the MonoRail framework.
scaffold | Override scaffolding support with a custom implementation.
services | Overrides default MonoRail services with your own implementations.
url | Whether or not URLs will use extensions.
viewcomponents | Registers assemblies that contain view components.
viewEngines | Registers and configures view engines.
additionalSources | Registers additional view sources.

## The `<additionalSources>` Element

The `<additionalSources>` element allows you to register additional paths from which view sources can be read as well as assemblies that contain view sources as embedded resources.

This node does not support any attributes and expects one or more child `<assembly>` or `<path`> elements.

```xml
  <additionalSources>
    <!--
    Assemblies that contain views as embedded resources.
    -->
    <assembly name="MyViewAssembly" namespace="My.Views" />
    <assembly name="MyOtherViewAssembly" namespace="My.Other.Views" />

    <!--
    Relative paths to folders in the website that contain view sources.
    -->
    <path location="/OtherViewSources" />
    <path location="/SpecialViews" />
  </additionalSources>
```

The `<assembly>` element is used for registering an assembly that contains view sources as embedded resources; it supports the following attributes:

Attribute | Description
----------|------------
name | The name of the assembly that contains the view sources as embedded resources.
namespace | The namespace in which the resource is located in the assembly.

Resources can have a namespace in their names. The namespace must be provided so MonoRail can remove the value to find the view content as though it was in the file system.

The `<path>` element is used for registering a path relative to the root of the application that contains view sources; it supports the following attributes:

Attribute | Description
----------|------------
location | The relative path from the root of the application that contains the view sources.

**Embedded Resources:** If a view engine implementation allows you to use assemblies with embedded resources to store views this can be a useful approach for keeping views with controllers so that they can be packaged and reused among projects.

## The `<controllers>` Element

The `<controllers>` element provides support for registering a collection of assembly names that contain the controller types to be registered with MonoRail.  During initialization the MonoRail controller factory will construct a tree of controllers from the types in the registered assemblies that implement the `IController` interface.

The `<controllers>` element has no attributes but can contain one or more `<assembly>` elements.

```xml
<controllers>
  <assembly>MyControllerAssembly</assembly>
  <assembly>MyOtherControllerAssembly</assembly>
</controllers>
```

The `<assembly>` element has no attributes or child elements and the inner text of the element must contain the name of an assembly that should be inspected for controller types.

**Node usage:** The `<controllers>` element is only used by the default controller factory and may be ignored by other factories. For example, if Windsor Container integration is enabled, the node will be ignored.

## The `<extensions>` Element

The `<extensions>` element allows a collection of extension types the implement the `IMonoRailExtension` interface to be registered with MonoRail.

This element has no attributes but supports one or more child `<extension>` elements, each of which registers an extension.

```xml
<extensions>
  <extension type="My.Extension, MyAssembly" />
  <extension type="Castle.MonoRail.Framework.Extensions.ExceptionChaining.ExceptionChainingExtension, Castle.MonoRail.Framework" />
</extensions>
```

Each `<extension>` element must have a `type` attribute that contains the fully qualified name of the extension type to register.

**Additional Information:** For further information on extensions see the [Extensions](extensions.md) section of this documentation.

## The `<scaffold>` Element

The `<scaffold>` allows you to override the default type used for scaffolding support.

```xml
<scaffold type="My.CustomScaffoldSupportType, MyAssembly" />
```

The element does not support any child elements and expects a single `type` attribute which should be set with the fully qualified name of a type that implements `IScaffoldingSupport`.

**Additional Information:** For further information on scaffolding see the [Scaffolding](scaffolding.md) section of this documentation.

## The `<services>` Element

MonoRail is composed of several pre-registered services, each of which is stored in an internal service registry.  The `<services>` element allows you to register your own services into the service registry.

The `<services>` element does  have any attributes but does support one or more child `<service>` elements.

```xml
<services>

  <!--
  Override the controller factory.
  -->
  <service
    id="ControllerFactory"
    type="My.CustomControllerFactory, MyAssembly" />

  <!--
  Register new services.
  -->
  <service
    id="Custom"
    type="My.CustomService, MyAssembly"
    interface="My.ICustomService, MyAssembly" />

  <service
    id="Custom"
    type="My.OtherCustomService, MyAssembly"
    interface="My.IOtherCustomService, MyAssembly" />

</services>
```

Each of the `<service>` elements supports the following attributes:

Attribute | Description
----------|------------
id | The service id used internally by MonoRail
type | The fully qualified type name of the type that implements the service.
interface | If the service is defined by an interface contract this attribute specifies the fully qualified type name of the interface.

The `id` attribute is mandatory and is strictly limited to the values in the list below.

* Custom
* ControllerFactory
* ViewEngine
* ViewSourceLoader
* ViewComponentFactory
* FilterFactory
* ResourceFactory
* EmailSender
* ControllerDescriptorProvider
* ResourceDescriptorProvider
* RescueDescriptorProvider
* LayoutDescriptorProvider
* HelperDescriptorProvider
* FilterDescriptorProvider
* EmailTemplateService
* ControllerTree
* CacheProvider
* ScaffoldingSupport
* ExecutorFactory
* TransformFilterDescriptorProvider
* TransformationFilterFactory
* ViewEngineManager
* UrlBuilder
* UrlTokenizer
* ServerUtility
* ValidatorRegistry
* AjaxProxyGenerator

Each of these `id` values correspond to a pre-existing MonoRail service which you can override, with the exception of the value `Custom`, which allows you to register your own custom services.

The `type` attribute is also mandatory and must contain the fully qualified name of the type to be registered as a service.

The `interface` attribute specifies the interface for the service.  This attribute is optional for services where the `id` is not `Custom` as MonoRail will automatically infer the interface based on the `id` value; for a service registration whose `id` equals `Custom` the `interface` attribute must be provided.  When the `interface` attribute is specified it must contain the fully qualified name of the interface type and the type specified in the `type` attribute must implement the specified interface.

**Additional Information:** For further information on services see the [Service Architecture](service-architecture.md) section of this documentation.

## The `<url>` Element

The `<url>` element controls whether or not URLs sent to and generated by MonoRail should have extensions.

```xml
<url useExtensions="false" />
```

This element does not support any child elements and only supports a single attribute, `useExtensions` that accepts the values "true" and "false".  If this element is not provided MonoRail will use extensions by default.

## The `<viewcomponents>` Element

The `<viewcomponents>` element is used to register additional assemblies that contain view components.

This element has no attributes but expects one or more child `assembly` nodes, each of which specifies the name of an assembly containing view components.

```xml
<viewcomponents>
  <assembly>MyViewComponentAssembly</assembly>
  <assembly>MyOtherViewComponentAssembly</assembly>
</viewcomponents>
```

During initialization MonoRail will inspect each of the assemblies provided for types that extend the `ViewComponent` base class and will register them for use at runtime.

**Node usage:** This node is only used by the default controller factory. It may be ignored by different factories. For example, if Windsor Container integration is enabled, the node will be ignored.

**Additional Information:** For further information on view components see the [View Components](view-components.md) section of this documentation.

## The `<viewEngines>` Element

The `<viewEngines>` element is used for registering and configuring the view engines used for rendering views. If no view engines are registered the default `WebForms` view engine will be used.

```xml
<viewEngines viewPathRoot="ViewSources">
  <add type="Castle.MonoRail.Framework.Views.NVelocity.NVelocityViewEngine, Castle.MonoRail.Framework.Views.NVelocity" xhtml="true"/>
  <add type="Castle.MonoRail.Views.Spark.SparkViewFactory, Castle.MonoRail.Views.Spark"/>
</viewEngines>
```

This element supports a single attribute `viewPathRoot` which provides support for specifying a custom root path from which view files will read.  If this attribute is omitted the default value of "views" will be used.

In addition, the `<viewEngines>` element expects one or more child `<add>` elements, each of which registers a view engine.  The `<add>` element supports the following attributes:

Attribute | Description
----------|------------
type | The fully qualified type name of a type that implements the `IViewEngine` interface.
xhtml | Whether or not the view engine enforces XHTML compliance or not. Valid values are "true" and "false".

Valid values for the `xhtml` attribute are `true` and `false` and the default value if the attribute is omitted is `false`.

**Additional Information:** For further information on view engines see the [View Engines](view-engines.md) section of this documentation.