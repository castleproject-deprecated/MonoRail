namespace Castle.Blade.Web

    open System
    open System.Web
    open System.Web.Compilation
    open Castle.Blade

    type BladeWebEngineHost() = 
        inherit BladeEngineHost()

        do 
            let codeOpts = base.CodeGenOptions
            let imports = codeOpts.Imports
            imports.Add "System"
            imports.Add "System.Collections.Generic"
            imports.Add "System.IO"
            imports.Add "System.Linq"
            imports.Add "System.Net"
            imports.Add "System.Web"
            imports.Add "System.Web.Security"



(*
  <configSections>
    <sectionGroup name="system.web.webPages.razor" type="System.Web.WebPages.Razor.Configuration.RazorWebSectionGroup, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
      <section name="host" type="System.Web.WebPages.Razor.Configuration.HostSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
      <section name="pages" type="System.Web.WebPages.Razor.Configuration.RazorPagesSection, System.Web.WebPages.Razor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
    </sectionGroup>
  </configSections>

  <system.web.webPages.razor>
    <host factoryType="Castle.MonoRail.ViewEngines.Razor.MonoRailRazorHostFactory, Castle.MonoRail.ViewEngines.Razor" />
    <pages pageBaseType="Castle.MonoRail.Razor.WebViewPage">
      <namespaces>
        <add namespace="Castle.MonoRail" />
        <add namespace="Castle.MonoRail.Helpers" />
				<add namespace="Castle.MonoRail.Routing" />
      </namespaces>
    </pages>
  </system.web.webPages.razor>
 *)

        

