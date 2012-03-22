
let targetFolder = @".\build\"

#I @"build\"
#r "System.ComponentModel.Composition.Codeplex.dll"
#r "Castle.MonoRail.dll"

open System.Reflection
open System.IO
open System.ComponentModel.Composition
open Castle.MonoRail.Framework

let asm = Assembly.LoadFrom(Path.Combine(targetFolder, "Castle.MonoRail.dll"))

let loaded_types = 
    try
        asm.GetTypes()
    with
    | :? ReflectionTypeLoadException as ex -> ex.Types

let exports = 
    loaded_types 
    |> Seq.filter (fun t -> t <> null && t.GetCustomAttributes(typeof<ExportAttribute>, true).Length <> 0 )

let appcomponents = 
    exports
    |> Seq.filter (fun t -> not (t.IsDefined(typeof<PartMetadataAttribute>, true)) || 
                            (t.GetCustomAttributes(typeof<PartMetadataAttribute>, true).[0] :?> PartMetadataAttribute).Value :?> ComponentScope = ComponentScope.Application )
    
let reqcomponents = 
    exports
    |> Seq.filter (fun t -> (t.IsDefined(typeof<PartMetadataAttribute>, true)) &&  
                            (t.GetCustomAttributes(typeof<PartMetadataAttribute>, true).[0] :?> PartMetadataAttribute).Value :?> ComponentScope = ComponentScope.Request )

printfn "App level components: "
for t in appcomponents do
    printfn "   %s" t.FullName
            
printfn "Req level components: "
for t in reqcomponents do
    printfn "   %s" t.FullName

