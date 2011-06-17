namespace Castle.Blade.Web

    open System
    open System.Configuration
    open System.Web
    open System.Web.Configuration
    open Castle.Blade

(*
  <configSections>
    <sectionGroup name="system.web.castle.blade" type="Castle.Blade.Web.BladeWebSectionGroup, Castle.Blade.Web, Version=1.0.0.0, Culture=neutral">
      <section name="pages" type="Castle.Blade.Web.BladePagesSection, Castle.Blade.Web, Version=1.0.0.0, Culture=neutral" requirePermission="false" />
    </sectionGroup>
  </configSections>

  <system.web.castle.blade>
    <pages pageBaseType="Castle.MonoRail.Blade.WebViewPage">
      <namespaces>
        <add namespace="Castle.MonoRail" />
        <add namespace="Castle.MonoRail.Helpers" />
		<add namespace="Castle.MonoRail.Routing" />
      </namespaces>
    </pages>
  </system.web.castle.blade>
 *)

    type BladeSectionGroup() = 
        inherit ConfigurationSectionGroup()

        [<ConfigurationProperty("pages", IsRequired=false)>]
        member x.Pages =
            x.Sections.["pages"] :?> BladePagesSection
        

    and BladePagesSection() = 
        inherit ConfigurationSection()
        
        let _pageBaseAttribute   = ConfigurationProperty("pageBaseType", typeof<string>, null, ConfigurationPropertyOptions.IsRequired)
        let _namespacesAttribute = ConfigurationProperty("namespaces", typeof<NamespaceCollection>, null, ConfigurationPropertyOptions.IsRequired)

        [<ConfigurationProperty("pageBaseType", IsRequired=true)>]
        member this.PageBaseType = string(this.[_pageBaseAttribute])
            
        [<ConfigurationProperty("namespaces", IsRequired=true)>]
        member this.Namespaces = this.[_namespacesAttribute] :?> NamespaceCollection
        

