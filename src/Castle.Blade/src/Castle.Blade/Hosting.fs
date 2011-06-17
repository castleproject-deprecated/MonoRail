namespace Castle.Blade 

    open System
    open System.IO
    open System.Collections.Generic
    

    type BladeEngineHost() = 
        let mutable _codeGenOptions = CodeGenOptions()

        member x.CodeGenOptions 
            with get() = _codeGenOptions and set v = _codeGenOptions <- v

        member x.GenerateCode (reader:TextReader, generatedFileName:string, sourcefileName:string) = 
            let nodes = Parser.parse_from_reader reader sourcefileName
            let compilationUnit = CodeGen.GenerateCodeFromAST generatedFileName nodes _codeGenOptions
            compilationUnit
        

