# XML Configuration Sample

Below is a sample XML fragment that demonstrates all of the elements and attributes supported by the [MonoRail configuration schema](xml-configuration-schema.md).

```xml
<!--
The main monorail configuration node.
-->
<monorail xmlns="urn:castle-monorail-configuration-2.0"
    smtpHost="smtp.host.com" smtpPort="443"
    smtpUsername="myuser"  smtpPassword="mypass"
    smtpUseSsl="true"
    >

  <!--
      Specify additional locations in which view source can be found
    -->
  <additionalSources>

    <!--
        Assemblies that contain views as embedded resources.
      -->
    <assembly name="MyViewAssembly" namespace="My.Views"/>
    <assembly name="MyOtherViewAssembly" namespace="My.Other.Views"/>

    <!--
        Relative paths to folders in the website that contain view sources.
      -->
    <path location="/OtherViewSources"/>
    <path location="/SpecialViews"/>

  </additionalSources>

  <!--
    Register assemblies that contain controllers.
  -->
  <controllers>
    <assembly>MyControllerAssembly</assembly>
    <assembly>MyOtherControllerAssembly</assembly>
  </controllers>

  <!--
    Register extensions.
  -->
  <extensions>
    <extension type="My.Extension, MyAssembly" />
    <extension type="Castle.MonoRail.Framework.Extensions.ExceptionChaining.ExceptionChainingExtension, Castle.MonoRail.Framework" />
  </extensions>

  <!--
    Override scaffolding support with a custom implementation.
  -->
  <scaffold type="My.CustomScaffoldSupportType, MyAssembly"/>

  <!--
    Register a service or override a built in services.

    Supported id values:

            Custom
            ControllerFactory
            ViewEngine
            ViewSourceLoader
            ViewComponentFactory
            FilterFactory
            ResourceFactory
            EmailSender
            ControllerDescriptorProvider
            ResourceDescriptorProvider
            RescueDescriptorProvider
            LayoutDescriptorProvider
            HelperDescriptorProvider
            FilterDescriptorProvider
            EmailTemplateService
            ControllerTree
            CacheProvider
            ScaffoldingSupport
            ExecutorFactory
            TransformFilterDescriptorProvider
            TransformationFilterFactory
            ViewEngineManager
            UrlBuilder
            UrlTokenizer
            ServerUtility
            ValidatorRegistry
            AjaxProxyGenerator
  -->
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
       interface="My.ICustomService, MyAssembly"/>

    <service
      id="Custom"
      type="My.OtherCustomService, MyAssembly"
      interface="My.IOtherCustomService, MyAssembly"/>

  </services>

  <!--
    Disable the requirement for extensions on URLs
  -->
  <url useExtensions="false"/>

  <!--
    Register assemblies that contain view components.
  -->
  <viewcomponents>
    <assembly>MyViewComponentAssembly</assembly>
    <assembly>MyOtherViewComponentAssembly</assembly>
  </viewcomponents>

  <!--
    Register and configure view engines.
  -->
  <viewEngines viewPathRoot="ViewSources">
    <add type="Castle.MonoRail.Framework.Views.NVelocity.NVelocityViewEngine, Castle.MonoRail.Framework.Views.NVelocity" xhtml="true"/>
    <add type="Castle.MonoRail.Views.Spark.SparkViewFactory, Castle.MonoRail.Views.Spark"/>
  </viewEngines>

</monorail>
```