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

        type Entry = {
            File : string; 
            MoveBy : int; 
            Index : int;
            mutable Type : string;
        }

        type PropertyGroup = {
            Configuration : string; 
            Properties : (string * string) seq;
        }

        type FscParams = {
            mutable OutputFolder : string;
            AsmName : string;
            References : string seq;
            SourceFiles : string seq;
            Properties : (string * string) seq;
            Defines : string seq;
            OutputType : string;
        }

        let internal quoteIfNeeded (s:string) = 
            if s.IndexOf(' ') <> -1 then sprintf "\"%s\"" s else s

        let internal serializeFSCParams (p: FscParams) = 
            let ext = 
                match p.OutputType with
                | "Exe" -> ".exe"
                | "Library" -> ".dll" 
                | _ -> failwith "Unsupported target type"
            let initialSet = 
                sprintf "-o:" + Path.Combine(p.OutputFolder, p.AsmName + ext) + 
                " -g --debug:full --noframework " + 
                "--optimize- --tailcalls- " +
                sprintf "--target:%s " (p.OutputType.ToLowerInvariant()) + 
                "--warn:3 --warnaserror:76 --vserrors --LCID:1033 --utf8output --fullpaths --flaterrors "
            let defines = 
                p.Defines |> Seq.map (fun d -> "--define:" + d) |> separated " "
            let references = 
                p.References 
                |> Seq.map (fun r -> sprintf "-r \"%s\"" r)
                |> separated " "
            let sourceFiles = 
                p.SourceFiles
                |> Seq.map (fun r -> sprintf "%s" <| quoteIfNeeded r)
                |> separated " "
            sprintf "%s %s %s %s" initialSet defines references sourceFiles

        let internal getReferenceElements projectFileName (doc:XDocument) outputFolder =
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
            let all = 
                items 
                |> Seq.map (fun e -> (e, e.Name.LocalName, e.Attribute(XName.Get "Include")))
                |> Seq.filter (fun (e,t,_) -> t = "None" || t = "Content" || t = "Compile" )
                |> Seq.map (fun (e,t,att) -> (e, t, att.Value))

            let files = 
                all 
                |> Seq.mapi(fun ind (e,typ,sourceFile) -> 
                    let ordering = 
                        let desc = e.Descendants(xname "move-by")
                        if (Seq.isEmpty <| desc) then null else desc.First().Value
                    if (ordering <> null) then
                        (sourceFile, Convert.ToInt32(ordering), typ)
                    else
                        (sourceFile, -1, typ)
                    ) |> Seq.toArray

            let newList = List<string>( files |> Seq.map (fun (file, _, _) -> file) )
            let name2Type = all |> Seq.map (fun (_, typ, file) -> (file, typ) ) |> Map.ofSeq

            let entries =
                files
                |> Seq.mapi (fun i (file, order, typ) -> 
                                    { 
                                        File = file; 
                                        MoveBy = order; 
                                        Index = i; 
                                        Type = name2Type.[file]
                                    }
                            )
            
            // when we process the entries, we need all nodes that can have a moveBy
            entries
            |> Seq.filter (fun e -> e.MoveBy <> -1)
            |> Seq.sortBy (fun e -> -e.Index)
            |> Seq.iter (fun e -> 
                            newList.Remove e.File |> ignore
                            newList.Insert ((e.Index + e.MoveBy), e.File)
                        )

            // Remove None/Content file names
            name2Type |> Map.iter (fun k v -> if v <> "Compile" then newList.Remove(k) |> ignore)

            newList |> box :?> string seq

        let internal build project parames = 
            traceStartTask "FSC" project
            let args = parames |> serializeFSCParams
            tracefn "Building project: %s\n  %s %s" project fscExe args
            if not (execProcess3 (fun info ->  
                                    info.WorkingDirectory <- (fileInfo project).DirectoryName
                                    info.FileName <- fscExe
                                    info.Arguments <- args) TimeSpan.MaxValue
                   )
            then failwithf "Building %s project failed." project
            traceEndTask "FSC" project

        let internal deserialize projectFile (proj:XDocument) =
            let groups = proj.Descendants(xname "PropertyGroup")
            let pGroups = 
                groups |> Seq.map (fun pg -> 
                                        let condition = 
                                            let att = pg.Attribute(XName.Get "Condition")
                                            if att <> null then
                                              let parts = att.Value.Split([|"=="|], StringSplitOptions.RemoveEmptyEntries)
                                              parts.[1].Trim()
                                            else
                                               "all"
                                        let props = 
                                            pg.Descendants() |> Seq.map (fun el -> (el.Name.LocalName, el.Value))

                                        { Configuration = condition; Properties = props }
                                  )
            let basePropertyGroup = pGroups |> Seq.find (fun e -> e.Configuration = "all" )
            let asmName = basePropertyGroup.Properties |> Seq.find (fun t -> fst t = "AssemblyName") |> snd
            let outputType = basePropertyGroup.Properties |> Seq.find (fun t -> fst t = "OutputType") |> snd
            let config = basePropertyGroup.Properties |> Seq.find (fun t -> fst t = "Configuration") |> snd
            let outType = basePropertyGroup.Properties |> Seq.find (fun t -> fst t = "OutputType") |> snd
            let props = 
                pGroups 
                |> Seq.filter (fun g -> g.Configuration = "all" || g.Configuration.IndexOf(config, StringComparison.OrdinalIgnoreCase) <> -1) 
                |> Seq.collect (fun g -> g.Properties)
            let outputFolder = props |> Seq.find (fun t -> fst t = "OutputPath") |> snd
            let constants = props |> Seq.find (fun t -> fst t = "DefineConstants") |> snd

            let targets = proj
            let references = getReferenceElements projectFile proj outputFolder
            let sourceFiles = getSourceFiles proj

            { 
                References = references; 
                SourceFiles = sourceFiles; 
                OutputFolder = outputFolder; 
                AsmName = asmName; 
                Properties = props;
                Defines = constants.Split(';');
                OutputType = outType;
            }

        let Execute (outputFolder:string) (projects:string seq) = 
            let outputFolder = (DirectoryInfo outputFolder).FullName
            for project in projects do
                let proj = loadProject project
                let fscParams = deserialize project proj
                fscParams.OutputFolder <- outputFolder
                build project fscParams

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
