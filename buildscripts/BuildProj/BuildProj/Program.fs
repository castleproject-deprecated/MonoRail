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
            References : list<string>;
            Parameters : list<string>;
            SourceFiles : list<string>;
        }

        type Entry = {
            File : string; MoveBy : int; Index : int;
        }

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
                        Path.Combine (hint, refAssembly)
                    else
                        refAssembly
                    ) 

        let internal getSourceFiles projectFileName (doc:XDocument) =
            let fi = fileInfo projectFileName
            let groups = 
                doc.Descendants(xname "Project")
                   .Descendants(xname "ItemGroup")
            let items   = groups.Descendants() 
            let all = items |> Seq.filter (fun e -> e.Name.LocalName = "None" || e.Name.LocalName = "Content" || e.Name.LocalName = "Compile" )

            all |> Seq.mapi(fun ind e -> 
                    let a = e.Attribute(XName.Get "Include")
                    let ordering : string = 
                        let desc = e.Descendants(xname "move-by")
                        if (Seq.isEmpty <| desc) then null else desc.First().Value
                    let sourceFile = a.Value
                    
                    if (ordering <> null) then
                        (sourceFile, Convert.ToInt32(ordering))
                    else
                        (sourceFile, -1)
                    ) |> Seq.toArray

        let orderSourceList (sourceFiles:(string*int) seq) = 
            let input = 
                sourceFiles 
                |> Seq.map (fun t -> fst t)

            let newList = List<string>( input )
            
            sourceFiles
            |> Seq.mapi (fun i tup -> { File = fst tup; MoveBy = snd tup; Index = i })
            |> Seq.filter (fun e -> e.MoveBy <> -1)
            |> Seq.sortBy (fun e -> -e.Index)
            |> Seq.iter (fun e -> 
                            newList.Remove e.File |> ignore
                            newList.Insert ((e.Index + e.MoveBy), e.File)
                            ()
                         )
            newList |> box :?> string seq

        let Execute (projFile:string) (projects:string seq) = 
            // let ev = environVar "MSBuild"
            for project in projects do
                let proj = loadProject project
                getReferenceElements project proj |> ignore // |> Seq.iter (fun e -> logfn "%s" e)
                let sourceFiles = getSourceFiles project proj // |> Seq.iter (fun e -> logfn "Source %s Order %d" (fst e) (snd e) )
                let ordered = orderSourceList sourceFiles
                ordered |> Seq.iter (fun e -> logfn "Source %s" e )
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
    FscTask.Execute buildDir appReferences
    (* 
    MSBuildDebug buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
    *)
)

(*

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
  ==> "Build" // <=> "BuildTest"
//  ==> "FxCop"
//  ==> "NUnitTest"
  ==> "Deploy"

// start build
Run "Deploy"
// RunParameterTargetOrDefault "target" "Deploy"
