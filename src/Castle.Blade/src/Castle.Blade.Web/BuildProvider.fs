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
    open System.Web
    open System.Web.Compilation

    [<BuildProviderAppliesTo( BuildProviderAppliesTo.Web )>]
    type BladeBuildProvider() = 
        inherit BuildProvider() 

        override x.CodeCompilerType = 
            x.GetDefaultCompilerTypeForLanguage("csharp")

        // override x.VirtualPathDependencies = 
        
        override x.GetGeneratedType(result) = 
            // result.CompiledAssembly.GetType
            null

        override this.GenerateCode(builder) = 
            // builder.AddCodeCompileUnit (this, G
            ()

