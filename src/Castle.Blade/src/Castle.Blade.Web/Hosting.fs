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




    type BladeWebEngineHostFactory = 
        class

        end



