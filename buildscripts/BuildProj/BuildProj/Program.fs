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


        let orderSourceList (sourceFiles:(string*int) seq) = 
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
