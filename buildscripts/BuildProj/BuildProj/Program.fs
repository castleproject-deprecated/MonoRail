open Fake
open System
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Linq
open System.Xml
open System.Xml.Linq
open System.Text

module FscTask = 
    begin
        
        let fscExe =   
            let ev = environVar "FSC"
            if not (isNullOrEmpty ev) then ev else
                findPath "FSCPath" "fsc.exe"

        let assemblyName2FileName = 
            let dict = Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
            let fxRefFolderRoot = Path.Combine ( Environment.GetFolderPath Environment.SpecialFolder.ProgramFilesX86, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" )
            for file in Directory.GetFiles(fxRefFolderRoot, "*.dll") do
                dict.[Path.GetFileNameWithoutExtension(file)] <- file
            let fsRefFolderRoot = Path.Combine ( Environment.GetFolderPath Environment.SpecialFolder.ProgramFilesX86, @"Reference Assemblies\Microsoft\FSharp\2.0\Runtime\v4.0" )
            for file in Directory.GetFiles(fsRefFolderRoot, "*.dll") do
                dict.[Path.GetFileNameWithoutExtension(file)] <- file
            dict

        type FscParams = {
            References : string seq;
            SourceFiles : string seq;
            OutputFolder : string;
            AsmName : string
            // Parameters : list<string>;
            // Defines : list<string>;
        }

        type Entry = {
            File : string; MoveBy : int; Index : int;
        }

        let internal quoteIfNeeded (s:string) = 
            if s.IndexOf(' ') <> -1 then sprintf "\"%s\"" s else s

        let internal serializeFSCParams (p: FscParams) = 
            let initialSet = 
                sprintf "-o:" + Path.Combine(p.OutputFolder, p.Name) + " "
                // "-o:obj\Debug\Castle.MonoRail.dll " + 
                "-g --debug:full --noframework --define:DEBUG --define:TRACE " + 
                // "--doc:C:\dev\github\castle\build\Castle.MonoRail.XML " + 
                "--optimize- --tailcalls- " +
                "--target:library --warn:3 --warnaserror:76 --vserrors --LCID:1033 --utf8output --fullpaths --flaterrors "
            let references = 
                p.References 
                |> Seq.map (fun r -> sprintf "-r \"%s\"" r)
                |> separated " "
            let sourceFiles = 
                p.SourceFiles
                |> Seq.map (fun r -> sprintf "%s" <| quoteIfNeeded r)
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

        let internal getReferenceElements projectFileName (doc:XDocument) outputFolder =
            (* 
          <ItemGroup>
            <ProjectReference Include="..\Castle.Blade\Castle.Blade.fsproj">
              <Name>Castle.Blade</Name>
              <Project>{51fc792e-69d3-41c4-937e-4abac181d918}</Project>
              <Private>True</Private>
            </ProjectReference>
          </ItemGroup>
            *)
            let fi = fileInfo projectFileName
            let groups = doc.Descendants(xname "Project")
                            .Descendants(xname "ItemGroup")
            let items = groups.Descendants() 
            let all   = items |> Seq.filter (fun e -> e.Name.LocalName = "Reference" || e.Name.LocalName = "ProjectReference" )
            all |> Seq.map(fun e -> 
                        let a = e.Attribute(XName.Get "Include")
                        let refAssembly = a.Value

                        if e.Name.LocalName = "ProjectReference" then
                            let asmName = e.Elements(xname "Name").First().Value
                            Path.Combine (outputFolder, asmName + ".dll")
                        else 
                            let hint : string = 
                                let desc = e.Descendants(xname "HintPath")
                                if (Seq.isEmpty <| desc) then 
                                    null
                                else
                                    desc.First().Value
                            if (hint <> null) then
                                (fileInfo <| Path.Combine(fi.Directory.FullName, hint)).FullName
                            else
                                let res, file = assemblyName2FileName.TryGetValue(refAssembly)
                                if not res then
                                    failwithf "Could not find full reference to assembly %s" refAssembly
                                file
                        ) 

        let internal getSourceFiles (doc:XDocument)  = 
            let groups = 
                doc.Descendants(xname "Project")
                   .Descendants(xname "ItemGroup")
            let items = groups.Descendants() 
            let all   = items |> Seq.filter (fun e -> e.Name.LocalName = "None" || e.Name.LocalName = "Content" || e.Name.LocalName = "Compile" )
            let files = 
                all 
                |> Seq.mapi(fun ind e -> 
                    let a = e.Attribute(XName.Get "Include")
                    let ordering = 
                        let desc = e.Descendants(xname "move-by")
                        if (Seq.isEmpty <| desc) then null else desc.First().Value
                    let sourceFile = a.Value
                    if (ordering <> null) then
                        (sourceFile, Convert.ToInt32(ordering))
                    else
                        (sourceFile, -1)
                    ) |> Seq.toArray

            let input = files |> Seq.map (fun t -> fst t)
            let newList = List<string>( input )
            
            files
            |> Seq.mapi (fun i tup -> { File = fst tup; MoveBy = snd tup; Index = i })
            |> Seq.filter (fun e -> e.MoveBy <> -1)
            |> Seq.sortBy (fun e -> -e.Index)
            |> Seq.iter (fun e -> 
                            newList.Remove e.File |> ignore
                            newList.Insert ((e.Index + e.MoveBy), e.File)
                        )
            newList |> box :?> string seq

        let internal build (outputFolder:string) project parames = 
            traceStartTask "FSC" project
            let args = parames |> serializeFSCParams
            tracefn "Building project: %s\n  %s %s" project fscExe args
            if not (execProcess3 (fun info ->  
                info.WorkingDirectory <- (fileInfo project).DirectoryName
                info.FileName <- fscExe
                info.Arguments <- args) TimeSpan.MaxValue)
            then failwithf "Building %s project failed." project
            traceEndTask "FSC" project

        let Execute (outputFolder:string) (projects:string seq) = 
            let outputFolder = (DirectoryInfo outputFolder).FullName
            for project in projects do
                let proj = loadProject project
                let references = getReferenceElements project proj outputFolder
                let sourceFiles = getSourceFiles proj
                build outputFolder project { References = references; SourceFiles = sourceFiles; OutputFolder = outputFolder }

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
    FscTask.Execute buildDir appReferences
    // FscTask.Execute buildDir [|@"C:\dev\github\Castle.MonoRail3\src\Castle.MonoRail\Castle.MonoRail.fsproj"|]
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
