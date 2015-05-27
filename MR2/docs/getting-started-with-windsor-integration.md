# Getting Started with Windsor Integration

## Required Assemblies

Make your web project reference the following assemblies:

* `Castle.Core.dll`
* `Castle.Windsor.dll`
* `Castle.MonoRail.WindsorExtension.dll`

## Configuration

Use the `useWindsorIntegration` attribute:

```xml
<?xml version="1.0" ?>
<configuration>

    <configSections>
        <section name="monorail"
                 type="Castle.MonoRail.Engine.Configuration.MonoRailSectionHandler, Castle.MonoRail.Engine" />
        <section name="castle"
                 type="Castle.Windsor.Configuration.AppDomain.CastleSectionHandler, Castle.Windsor" />
    </configSections>

    <monorail useWindsorIntegration="true">
		<viewEngine
			viewPathRoot="views"
			customEngine="Castle.MonoRail.Framework.Views.NVelocity.NVelocityViewEngine, Castle.MonoRail.Framework.Views.NVelocity" />
    </monorail>

    <castle>

      <!-- component and facilities configuration goes here -->

    </castle>

</configuration>
```

## Exposing the Container

You must make your container available to the web application. The best place for it is `global.asax`:

```
<%@ Application Inherits="YourApp.Web.MyGlobalApplication"  %>
```

MyGlobalApplication.cs:

```csharp
namespace YourApp.Web
{
    using System;
    using System.Web;
    using Castle.Windsor;
    using Castle.ActiveRecord;

    public class MyGlobalApplication : HttpApplication, IContainerAccessor
    {
        private static WebAppContainer container;

        public void Application_OnStart()
        {
            container = new WebAppContainer();
        }

        public void Application_OnEnd()
        {
            container.Dispose();
        }

        public IWindsorContainer Container
        {
            get { return container; }
        }
    }
}
```

## Initializing

You must register a facility named `MonoRailFacility`. It ensures that the lifestyle of each controller is set to `Transient` - as using the default, `singleton`, would be a terrible mistake - and register each component/controller on the `ControllerTree`.

**Standard components:** Also note that from now on, your controllers, view components and optionally the filters are standard components, and they need to be registered on the container.

```csharp
namespace YourApp.Web
{
	using System;
	using YourApp.Web.Controllers;
	using Castle.Model.Resource;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.MonoRail.WindsorExtension;

	public class WebAppContainer : WindsorContainer
	{
		public WebAppContainer() : base(new XmlInterpreter( new ConfigResource() ))
		{
			RegisterFacilities();
			RegisterComponents();
		}

		protected void RegisterFacilities()
		{
			AddFacility<MonoRailFacility>();
		}

		protected void RegisterComponents()
		{
			Install(FromAssembly.Containing<HomeController>());
		}
	}
}
```

Obviously you can decide to register the facility and the components directly on the configuration file. The approach above is just a suggestion.