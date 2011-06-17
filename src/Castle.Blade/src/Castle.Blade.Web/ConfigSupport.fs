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
        

