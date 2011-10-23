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

        let mutable _pages = Unchecked.defaultof<BladePagesSection>

        static member GroupName = "castle.blade"

        [<ConfigurationProperty("pages", IsRequired=true)>]
        member x.Pages 
            with get() = _pages <|> lazy (x.Sections.["pages"] :?> BladePagesSection) 
            and set v = _pages <- v
        

    and BladePagesSection() = 
        inherit ConfigurationSection()
        
        
        // let mutable _pageBaseVal : string = null
        // let mutable _namespacesVal : NamespaceCollection = null

        static member SectionName = BladeSectionGroup.GroupName + "/pages"

        static member private pageBaseAttribute   = ConfigurationProperty("pageBaseType", typeof<string>, null, ConfigurationPropertyOptions.IsRequired)
        static member private namespacesAttribute = ConfigurationProperty("namespaces", typeof<NamespaceCollection>, null, ConfigurationPropertyOptions.IsRequired)

        [<ConfigurationProperty("pageBaseType", IsRequired=true)>]
        member this.PageBaseType 
            with get() = (string) this.[BladePagesSection.pageBaseAttribute]
             // _pageBaseVal <|> lazy string(this.[BladePagesSection.pageBaseAttribute])  this.Get
            // and set v = _pageBaseVal <- v
            
        [<ConfigurationProperty("namespaces", IsRequired=true)>]
        member this.Namespaces = this.[BladePagesSection.namespacesAttribute] :?> NamespaceCollection
        

