# Configuration

MonoRail requires some minimal configuration in your `web.config` file to function and once that's done its behavior can be configured through a variety of different approaches. This topic is intended to help you to understand the minimal requirements to get MonoRail up and running and to provide an overview of each of the approaches to configuring MonoRail.

## Basic Configuration

The following is a basic minimal XML configuration required to get MonoRail working with all defaults.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section
      name="monorail"
      type="Castle.MonoRail.Framework.Configuration.MonoRailSectionHandler, Castle.MonoRail.Framework" />
  </configSections>

  <monorail>
    <controllers>
      <assembly>ProjectAssembly</assembly>
    </controllers>
  </monorail>

  <system.web>
    <httpHandlers>
      <add
        verb="*"
        path="*.rails"
        type="Castle.MonoRail.Framework.MonoRailHttpHandlerFactory, Castle.MonoRail.Framework" />
    </httpHandlers>
  </system.web>
</configuration>
```

This configuration defaults to use the `WebForms` view engine to handle requests that end in `.rails` and uses an assembly named `ProjectAssembly` as the source of controllers and ViewComponents.

**Controllers Assembly:** This sample specifies the assembly that contains the controllers and associated resources in the `<controllers>` node; unfortunately the project assembly name cannot be inferred so it must be specified; you must replace _ProjectAssembly_ from the sample with the fully qualified name of your own controller assembly.}

Each of the nodes and sections in the above sample will be described in further detail in subsequent sections of this topic.

## ASP.NET Configuration

To use MonoRail for processing web requests some configuration of ASP.NET is required in your application's web.config file.

### Directing Requests to MonoRail

The minimum configuration in the web.config involves telling ASP.NET that requests associated with a specified extension (such as .rails) should be processed by MonoRail.  This is accomplished by adding an httpHandler to the `<httpHandlers>` node in the application's web.config file.

The following sample directs requests ending with the extension `.rails` to MonoRail:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.web>
    <httpHandlers>

      <add
        verb="*"
        path="*.rails"
        type="Castle.MonoRail.Framework.MonoRailHttpHandlerFactory, Castle.MonoRail.Framework" />

    </httpHandlers>
  </system.web>
</configuration>
```

The new httpHandler must specify the the extension to associate with MonoRail requests in the `path` attribute and the fully qualified name of the `MonoRailHttpHandlerFactory` in the type attribute as shown in the sample above.

**httpHandlers:** For further information on registering httpHanders in your web.config file please review the [Microsoft documentation on the subject](http://msdn.microsoft.com/en-us/library/46c5ddfy(v=VS.71).aspx).

### Prohibiting Access to View Source

In the event that any file not associated with your MonoRail extension is requested its contents will normally be sent directly to the calling browser, which may expose the source of view files you'd rather keep private. To prevent the source for your view files being visible to the outside world an `HttpForbiddenHandler` httpHandler can be configured for the file extension of any files you don't want want to be directly accessible.

The following sample prevents access to the source of NVelocity view files with the extension .vm being accessed.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.web>
    <httpHandlers>
      ...

      <add
        verb="*"
        path="*.vm"
        type="System.Web.HttpForbiddenHandler" />

    </httpHandlers>
  </system.web>
</configuration>
```

The new httpHandler must specify the the extension of the files you do not want to be acessible in the `path` attribute and the fully qualified name of the `HttpForbiddenHandler` in the `type` attribute.

## Configuring MonoRail

MonoRail can be configured using [XML configuration](xml-configuration.md) in the web.config of your application as the simple example earlier in this topic illustrates, [programatically](programmatic-configuration.md) or using a combination of the two approaches. When both XML and programmatic configuration are used together, the XML configuration will be loaded first and can then be supplemented or overridden in code.