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
    open System.Linq
    open System.Configuration
    open System.Web
    open System.Web.Configuration
    open System.Web.Compilation
    open Castle.Blade

    type BladeWebEngineHost(opts:CodeGenOptions) = 
        inherit BladeEngineHost(opts)

        do 
            let imports = opts.Imports
            imports.Add "System"
            imports.Add "System.Collections.Generic"
            imports.Add "System.IO"
            imports.Add "System.Linq"
            imports.Add "System.Net"
            imports.Add "System.Web"
            imports.Add "System.Web.Security"
            imports.Add "Castle.Blade"
            imports.Add "Castle.Blade.Web"


    type BladeWebEngineHostFactory() = 
        
        static member CreateFromConfig(vpath:string) = 
            let createOptsFromConfig (config:BladeSectionGroup) = 
                let pageConfig = config.Pages
                let opts = CodeGenOptions(DefaultNamespace = "ASP", DefaultBaseClass = "Castle.Blade.Web.WebBladePage")
                if pageConfig != null then
                    opts.DefaultBaseClass <- pageConfig.PageBaseType
                    let nameSpaces = pageConfig.Namespaces.Cast<NamespaceInfo>().Select( fun (nsInfo:NamespaceInfo) -> nsInfo.Namespace ) 
                    opts.Imports.AddRange nameSpaces
                opts
                    
            let getSectionGroup (vpath) = 
                let sec = WebConfigurationManager.GetSection(BladePagesSection.SectionName, vpath) :?> BladePagesSection
                BladeSectionGroup( Pages = sec )

            let config = getSectionGroup vpath
            let codeGenOptions = createOptsFromConfig config
            BladeWebEngineHost (codeGenOptions)


        



