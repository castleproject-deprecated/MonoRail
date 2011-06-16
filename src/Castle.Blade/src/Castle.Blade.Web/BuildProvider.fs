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

