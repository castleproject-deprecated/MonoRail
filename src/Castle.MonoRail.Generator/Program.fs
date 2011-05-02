// Learn more about F# at http://fsharp.net
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

open System
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Web
open System.CodeDom
open System.CodeDom.Compiler
open Castle.MonoRail.Routing
open Microsoft.CSharp

let inline (==) a b = Object.ReferenceEquals(a, b)
let inline (!=) a b = not (Object.ReferenceEquals(a, b))




let args = Environment.GetCommandLineArgs()

let curFolder : string = Environment.CurrentDirectory
let mutable binFolder : string = null
let mutable targetFolder : string = null

let (|Bin|Target|Invalid|) (input:string) = 
    if input.StartsWith "-b:" then
        Bin
    elif input.StartsWith "-t:" then
        Target
    else
        Invalid


for arg in args do
    match arg with
    | Bin -> binFolder <- Path.Combine(curFolder, arg.Substring(3))
    | Target -> targetFolder <- Path.Combine(curFolder, arg.Substring(3))
    | _ -> ignore()

let mutable inError = false
let prevColor = Console.ForegroundColor
    
if binFolder == null || not (Directory.Exists(binFolder)) then
    Console.ForegroundColor <- ConsoleColor.DarkRed
    Console.Error.WriteLine "Invalid bin folder. Specify a folder with -b:<folder>"
    inError <- true

if String.IsNullOrEmpty targetFolder then
    Console.ForegroundColor <- ConsoleColor.DarkRed
    Console.Error.WriteLine "Invalid target folder. Specify a folder with -t:<folder>"
    inError <- true

if inError then
    Console.ForegroundColor <- prevColor
    Console.WriteLine ""
    Console.WriteLine "Usage:"
    Console.WriteLine ""
    Console.WriteLine " mrtypegen.exe -b:<folder with assemblies> -t:<gen files folder>"
    Console.WriteLine ""
    Console.WriteLine "Example:"
    Console.WriteLine " mrtypegen.exe -b:WebApp\bin -t:WebApp\Generated"
    Console.WriteLine ""
    Environment.Exit -1

let blacklisted = ["Castle.MonoRail.dll";"Castle.MonoRail.Mvc.ViewEngines.Razor.dll";"Castle.MonoRail.Routing.dll";"System.ComponentModel.Composition.Codeplex.dll";"System.Reflection.Context.dll"]
let types = List<Type>()

for asmfile in Directory.GetFiles(binFolder, "*.dll") do
    
    let file = Path.GetFileName asmfile
    let found = Seq.exists (fun f -> f = file) blacklisted

    if not found then
        try
            Console.WriteLine ("Processing " + file)
            let asm = Assembly.LoadFrom asmfile

            let loaded_types = 
                try
                    asm.GetTypes()
                with
                | :? ReflectionTypeLoadException as ex -> ex.Types

            for t in loaded_types do
                if t != null then
                    types.Add t

        with 
        | ex -> Console.Error.WriteLine ("could not load " + asmfile)

let appTypes = 
    types
    |> Seq.filter (fun t -> t.BaseType == typeof<HttpApplication>)

let controllers = 
    types 
    |> Seq.filter (fun t -> t.Name.EndsWith("Controller"))
    |> Seq.map (fun t -> (t, t.Name.Substring(0, t.Name.Length - "Controller".Length)))

let flags = BindingFlags.DeclaredOnly|||BindingFlags.Public|||BindingFlags.NonPublic|||BindingFlags.Instance|||BindingFlags.Static

for app in appTypes do
    let appinstance = Activator.CreateInstance app :?> HttpApplication
    let startMethod = app.GetMethod("Application_Start", flags)
    if startMethod != null then
        // executing for side effects only, so we get the configured routes
        let args = 
            if startMethod.GetParameters().Length = 2 then
                [|appinstance;EventArgs.Empty|]:obj[]
            else
                [||]
        try
            if startMethod.IsStatic then
                startMethod.Invoke(null, args) |> ignore
            else 
                startMethod.Invoke(appinstance, args) |> ignore
        with 
        | ex -> 
            Console.Error.WriteLine ("could not run Application_Start for " + app.FullName)
            Console.Error.WriteLine "Routes may have not been evaluated"
            Console.Error.WriteLine (ex.ToString())

if Seq.isEmpty Router.Instance.Routes then
    Console.Error.WriteLine "No routes found"
    Environment.Exit -5


Console.WriteLine ""
Console.WriteLine "Routes discovered"
for r in Router.Instance.Routes do
    Console.WriteLine ("\t" + r.Name + "  " + r.Path)


let best_route_for controllerName (action:MethodInfo) = 
    try
        // this is likely to get very complex, especially when areas support is added
        let best_route = 
            Router.Instance.Routes
            |> Seq.find (fun r -> r.Path.StartsWith("/" + controllerName)) 
        best_route
    with
    | ex -> 
        let def = 
            Router.Instance.Routes
            |> Seq.find (fun r -> r.Name = "default") 
        def

type ActionDef(controller:Type, action:MethodInfo, route:Route) = 
    let _controller = controller
    let _action = action
    let _route = route
    
    member x.Generate (targetTypeDecl:CodeTypeDeclaration, imports) = 
        let fieldName = "_field"
        let field = CodeMemberField("TargetUrl", fieldName)
        field.Attributes <- MemberAttributes.Static

        let mmethod = CodeMemberMethod()
        mmethod.Name <- _action.Name
        mmethod.Attributes <- MemberAttributes.Static ||| MemberAttributes.Public
        mmethod.ReturnType <- CodeTypeReference("TargetUrl")

        for p in _action.GetParameters() do
            if p.ParameterType.IsPrimitive then
                let pDecl = CodeParameterDeclarationExpression(p.ParameterType, p.Name)
                mmethod.Parameters.Add pDecl |> ignore

        let fieldRef = CodeFieldReferenceExpression()
        fieldRef.FieldName <- fieldName

        let fieldAccessor = CodeMethodReturnStatement(fieldRef)
        mmethod.Statements.Add fieldAccessor |> ignore

        targetTypeDecl.Members.Add field |> ignore
        targetTypeDecl.Members.Add mmethod |> ignore
        ()

let controller2route = Dictionary<Type, List<ActionDef>>()

Console.WriteLine ""
for ct,name in controllers do
    Console.WriteLine ("Processing " + ct.FullName)
    
    let actions = 
        ct.GetMethods(BindingFlags.Instance|||BindingFlags.Public)
        |> Seq.filter (fun m -> not (m.DeclaringType == typeof<obj> || m.IsAbstract || m.IsSpecialName))

    let defs = List<ActionDef>()
    controller2route.[ct] <- defs

    for action in actions do    
        Console.WriteLine ("    Action: " + action.Name)
        let def = ActionDef(ct, action, (best_route_for name action))
        defs.Add def

let compilationUnit = CodeCompileUnit()
let imports = HashSet<string>()
imports.Add "System" |> ignore
imports.Add "Castle.MonoRail" |> ignore
imports.Add "Castle.MonoRail.Routing" |> ignore

for pair in controller2route do
    let ct = pair.Key
    let defs = pair.Value
    let ns = 
        let found = 
            compilationUnit.Namespaces 
            |> Seq.cast 
            |> Seq.filter (fun (ns:CodeNamespace) -> ns.Name = ct.Namespace)
        if Seq.isEmpty found then 
            let newns = CodeNamespace(ct.Namespace)
            compilationUnit.Namespaces.Add newns |> ignore
            newns
        else 
            Seq.head found
    
    // compilationUnit.Namespaces.Add 
    let typeDecl = CodeTypeDeclaration(ct.Name)
    typeDecl.TypeAttributes <- TypeAttributes.Public
    typeDecl.IsPartial <- true
    ns.Types.Add typeDecl |> ignore

    let staticType = CodeTypeDeclaration("Urls")
    staticType.TypeAttributes <- TypeAttributes.Abstract ||| TypeAttributes.Public
    typeDecl.Members.Add staticType |> ignore
    
    for def in defs do
        def.Generate(staticType, imports)
      

for import in imports do
    for ns in compilationUnit.Namespaces do
        ns.Imports.Add (CodeNamespaceImport(import)) |> ignore

let csprovider = new CSharpCodeProvider()

csprovider.GenerateCodeFromCompileUnit(compilationUnit, Console.Out, CodeGeneratorOptions())

