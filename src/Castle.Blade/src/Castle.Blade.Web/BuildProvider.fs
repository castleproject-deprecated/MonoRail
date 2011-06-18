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
    open System.IO
    open System.Web
    open System.Web.Compilation
    open Castle.Blade
    open Castle.Blade.Web

    [<BuildProviderAppliesTo( BuildProviderAppliesTo.Web )>]
    type BladeBuildProvider() = 
        inherit BuildProvider() 

        let mutable _host : BladeWebEngineHost = Unchecked.defaultof<_>

        let getHost (vppath:string) =
            if _host == null then
                _host <- BladeWebEngineHostFactory.CreateFromConfig(vppath)
            _host

        let generate_typename (vPath:string) = 
            let trimmed = vPath.TrimStart([|'~';'/'|])
            let file = Path.GetFileName trimmed
            let ns = trimmed.Substring(0, trimmed.Length - file.Length - 1).Replace('/', '.')
            ns, (Path.GetFileNameWithoutExtension file)

        override x.CodeCompilerType = 
            x.GetDefaultCompilerTypeForLanguage("csharp")

        override x.GetGeneratedType(result) = 
            let ns, typeName = generate_typename base.VirtualPath
            result.CompiledAssembly.GetType(ns + "." + typeName)

        override this.GenerateCode(builder) = 
            let vpath = base.VirtualPath
            let ns, typeName = generate_typename vpath
            let host = getHost(vpath)
            host.CodeGenOptions.DefaultNamespace <- ns
            use stream = base.OpenStream()
            let compilationUnit = getHost(vpath).GenerateCode (stream, typeName, base.VirtualPath, System.Text.Encoding.Default)
            builder.AddCodeCompileUnit (this, compilationUnit)
            builder.GenerateTypeFactory(ns + "." + typeName) 
