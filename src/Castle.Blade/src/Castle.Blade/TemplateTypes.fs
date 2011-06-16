namespace Castle.Blade

    open System
    open System.Collections.Generic
    
    [<AbstractClass>]
    type BaseBladePage() = 

        let _sections = Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase)

        abstract member RenderPage : unit -> unit 
        abstract member ConfigurePage : parent:BaseBladePage -> unit 

        member x.DefineSection(name:string, action:Action) =
            _sections.[name] <- action

        member x.RenderSection(name:string, required:bool) = 
            ()

        member x.RenderSection(name:string) = 
            x.RenderSection (name, true)

