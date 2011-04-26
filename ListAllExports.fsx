
let targetFolder = @".\src\Castle.MonoRail\bin\Debug"

#I @"src\Castle.MonoRail\bin\Debug"
#r "System.ComponentModel.Composition.Codeplex.dll"

open System.Reflection
open System.IO
open System.ComponentModel.Composition

let asm = Assembly.LoadFrom(Path.Combine(targetFolder, "Castle.MonoRail.dll"))

let loaded_types = 
    try
        asm.GetTypes()
    with
    | :? ReflectionTypeLoadException as ex -> ex.Types

let exports = 
    loaded_types 
    |> Seq.filter (fun t -> t <> null && t.GetCustomAttributes(typeof<ExportAttribute>, true).Length <> 0 )
    |> Seq.map (fun t -> (t, (t.GetCustomAttributes(typeof<ExportAttribute>, true).[0] :?> ExportAttribute).ContractType) )
    |> Seq.groupBy (fun (t,e) -> t.Namespace)

for (ns,types) in exports do
    printf "%s: \r\n" ns

    for t1,t2 in types do 
        if t2 = null then
            printf "\tContract: %s  implemented by  %s \r\n" t1.Name t1.Name
        else
            printf "\tContract: %s  implemented by  %s \r\n" t2.Name t1.Name

        let meta = 
            t1.GetCustomAttributes(typeof<ExportMetadataAttribute>, true) 
            |> Seq.map (fun m -> m :?> ExportMetadataAttribute)

        for m in meta do
            printf "\t   Metadata: %s  =  %s \r\n" m.Name (m.Value.ToString())
            

