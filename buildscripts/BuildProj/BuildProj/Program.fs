open Fake
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Xml
open System.Xml.Linq

module FscTask = 
    begin
        
        let fscExe =   
            let ev = environVar "FSC"
            if not (isNullOrEmpty ev) then ev else
                findPath "FSCPath" "fsc.exe"

        type FscParams = {
            References : string seq;
            // Parameters : list<string>;
            SourceFiles : string seq;
            // Defines : list<string>;
        }

        let serializeFSCParams (p: FscParams) = 
            let initialSet = 
                "-o:obj\Debug\Castle.MonoRail.dll -g --debug:full --noframework --define:DEBUG --define:TRACE " + 
                "--doc:C:\dev\github\castle\build\Castle.MonoRail.XML " + 
                "--optimize- --tailcalls- " +
                "--target:library --warn:3 --warnaserror:76 --vserrors --LCID:1033 --utf8output --fullpaths --flaterrors "
            let references = 
                p.References 
                |> Seq.map (fun r -> sprintf "-r:\"%s\"" r)
                |> separated " "
            let sourceFiles = 
                p.SourceFiles
                |> Seq.map (fun r -> sprintf "\"%s\"" r)
                |> separated " "
            initialSet + references + " " + sourceFiles

            (* 
            let targets = 
                match p.Targets with
                | [] -> None
                | t -> Some ("t", t |> separated ";")
            let properties = 
                p.Properties |> List.map (fun (k,v) -> Some ("p", sprintf "%s=\"%s\"" k v))
            let maxcpu = 
                match p.MaxCpuCount with
                | None -> None
                | Some x -> Some ("m", match x with Some v -> v.ToString() | _ -> "")
            let tools =
                match p.ToolsVersion with
                | None -> None
                | Some t -> Some ("tv", t)
            let verbosity = 
                match p.Verbosity with
                | None -> None
                | Some v -> 
                    let level = 
                        match v with
                        | Quiet -> "q"
                        | Minimal -> "m"
                        | Normal -> "n"
                        | Detailed -> "d"
                        | Diagnostic -> "diag"
                    Some ("v", level)
            let allParameters = [targets; maxcpu; tools; verbosity] @ properties
            allParameters
            |> Seq.map (function
                            | None -> ""
                            | Some (k,v) -> "/" + k + (if isNullOrEmpty v then "" else ":" + v))
            |> separated " "
            *)

        let internal getReferenceElements projectFileName (doc:XDocument) =
            let fi = fileInfo projectFileName
            doc.Descendants(xname "Project")
               .Descendants(xname "ItemGroup")
               .Descendants(xname "Reference")
             |> Seq.map(fun e -> 
                    let a = e.Attribute(XName.Get "Include")
                    let hint : string = 
                        let desc = e.Descendants(xname "HintPath")
                        if (Seq.isEmpty <| desc) then 
                            null
                        else
                            desc.First().Value
                    let refAssembly = a.Value
                    
                    if (hint <> null) then
                        (fileInfo <| Path.Combine(fi.Directory.FullName, hint)).FullName
                    else
                        refAssembly
                    ) 

        let getSourceFiles (doc:XDocument)  = 
            let sourceFiles = 
                doc.Descendants(xname "Project")
                   .Descendants(xname "ItemGroup")
                   .Descendants(xname "Compile")
                     |> Seq.map(fun e -> 
                                    let a = e.Attribute(XName.Get "Include")
                                    let ordering : string = 
                                        let desc = e.Descendants(xname "move-by")
                                        if (Seq.isEmpty <| desc) then null else desc.First().Value
                                    let sourceFile = a.Value
                                    if (ordering <> null) then
                                        (sourceFile, Convert.ToInt32(ordering))
                                    else
                                        (sourceFile, -1)
                               ) 

            let newList = List<string>( (sourceFiles |> Seq.map (fun t -> fst t)) )
            let swap file newIndex = 
                let old = newList.[newIndex]
                let index = newList.FindIndex( fun s -> s = file )
                newList.[newIndex] <- file
                newList.[index] <- old

            sourceFiles 
            |> Seq.iteri (fun ind tup -> 
                            let moveBy = snd tup
                            let file = fst tup
                            if moveBy <> -1 then
                                let index = newList.FindIndex( fun s -> s = file )
                                for i in 1..moveBy do
                                    swap file (index + i)
                                newList.Remove file |> ignore
                                newList.Insert (ind + moveBy, file)
                                ()
                         )
            newList |> box :?> string seq

        let internal build project parames = 
            traceStartTask "FSC" project
            let args = parames |> serializeFSCParams
            tracefn "Building project: %s\n  %s %s" project fscExe args
            if not (execProcess3 (fun info ->  
                info.FileName <- fscExe
                info.Arguments <- args) TimeSpan.MaxValue)
            then failwithf "Building %s project failed." project

            traceEndTask "FSC" project

        let Execute (projFile:string) (projects:string seq) = 
            // C:\Windows\Microsoft.NET\Framework\v4.0.30319
            // log <| Environment.GetFolderPath Environment.SpecialFolder.System
            
            // let ev = environVar "MSBuild"
            for project in projects do
                let proj = loadProject project
                let references = getReferenceElements project proj
                let sourceFiles = getSourceFiles proj
                build project { References = references; SourceFiles = sourceFiles }
            ()
            // ExecProcessWithLambdas infoAction (timeOut:TimeSpan) silent errorF messageF =

    end

// Directories
let buildDir  = @"..\build\"

// Tools
let nunitPath = @".\Tools\NUnit"
let fxCopRoot = @".\Tools\FxCop\FxCopCmd.exe"

// Filesets
let appReferences  = 
    Include @"src\Castle.Blade\src\**\*.fsproj" 
       ++ @"src\Castle.MonoRail\**\*.fsproj" 
       ++ @"src\Castle.MonoRail.Generator\**\*.fsproj" 
       ++ @"src\Castle.MonoRail.ViewEngines.Blade\**\*.fsproj" 
         |> SetBaseDir "../"
         |> ScanImmediately
// logfn "appReferences %A" appReferences

let testReferences = 
    !+ @"src\Castle.Blade\tests\**\*.csproj" 
       ++ @"tests\**\*.fsproj" 
       ++ @"tests\TestWebApp\*.csproj" 
         |> SetBaseDir "../"
         |> ScanImmediately
// logfn "testReferences %A" testReferences

Target "Clean" (fun _ -> 
    CleanDirs [buildDir]
)

Target "Build" (fun _ ->
    // compile all projects below src\app\
    // FscTask.Execute buildDir appReferences
    FscTask.Execute buildDir [|@"C:\dev\github\Castle.MonoRail3\src\Castle.MonoRail\Castle.MonoRail.fsproj"|]
    (* 
    MSBuildDebug buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
    *)
)

Target "BuildTest" (fun _ ->
    ()
    (* 
    MSBuildDebug buildDir "Build" testReferences
        |> Log "TestBuild-Output: "
    *)
)

Target "NUnitTest" (fun _ ->  
    !+ (buildDir + @"\*.Tests.dll") 
        |> Scan
        |> NUnit (fun p -> 
            {p with 
                ToolPath = nunitPath; 
                DisableShadowCopy = true; 
                OutputFile = buildDir + @"TestResults.xml"})
)

(*
Target "xUnitTest" (fun _ ->  
    !+ (testDir + @"\xUnit.Test.*.dll") 
        |> Scan
        |> xUnit (fun p -> 
            {p with 
                ShadowCopy = false;
                HtmlOutput = true;
                XmlOutput = true;
                OutputDir = testDir })
)
*) 

Target "FxCop" (fun _ ->
    ()
    (* 
    !+ (buildDir + @"\**\*.dll") 
        ++ (buildDir + @"\**\*.exe") 
        |> Scan  
        |> FxCop (fun p -> 
            {p with                     
                ReportFileName = buildDir + "FXCopResults.xml";
                ToolPath = fxCopRoot})
    *)
)

Target "Deploy" (fun _ ->
    ()
    (* 
    !+ (buildDir + "\**\*.*") 
        -- "*.zip" 
        |> Scan
        |> Zip buildDir (deployDir + "Calculator." + version + ".zip")
    *)
)


// Build order
"Clean"
  ==> "Build" <=> "BuildTest"
  ==> "FxCop"
  ==> "NUnitTest"
  ==> "Deploy"

// start build
Run "Deploy"
// RunParameterTargetOrDefault "target" "Deploy"
