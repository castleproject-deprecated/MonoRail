#I @"..\tools\FAKE"
#r "FakeLib.dll"
#r "System.Core.dll"
#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
open Fake
open System
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

        let internal getReferenceElements elementName projectFileName (doc:XDocument) =
            let fi = fileInfo projectFileName
            doc
              .Descendants(xname "Project")
              .Descendants(xname "ItemGroup")
              .Descendants(xname elementName)
                |> Seq.map(fun e -> 
                    let a = e.Attribute(XName.Get "Include")
                    let hint : string = 
                        let desc = e.Descendants(xname "HintPath")
                        if (Seq.isEmpty <| desc) then 
                            null
                        else
                            desc.First().Value

                    let value = a.Value

                    log hint
                    (*
                    let fileName =
                        if value.StartsWith(".." + directorySeparator) || (not <| value.Contains directorySeparator) then
                            fi.Directory.FullName @@ value
                        else
                            value
                    a,fileName |> FullName
                    *)
                    ) 

        let Execute (projFile:string) (projects:string seq) = 
            let ev = environVar "MSBuild"

            for project in projects do
                let proj = loadProject project
                getReferenceElements "Reference" projFile proj |> ignore

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
