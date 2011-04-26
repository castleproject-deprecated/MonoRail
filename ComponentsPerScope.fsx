
let targetFolder = @".\src\Castle.MonoRail\bin\Debug"

#I @"src\Castle.MonoRail\bin\Debug"
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

printf "App level components: \r\n"
for t in appcomponents do
    printf "   %s \r\n" t.FullName
            
printf "Req level components: \r\n"
for t in reqcomponents do
    printf "   %s \r\n" t.FullName

