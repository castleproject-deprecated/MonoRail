//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

module Generator

    open System
    open System.Collections.Generic
    open System.IO
    open System.Reflection
    open System.Web
    open System.CodeDom
    open System.CodeDom.Compiler
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Microsoft.CSharp
    open Castle.MonoRail.Generator.Api

    let args = Environment.GetCommandLineArgs()

    let curFolder : string = Environment.CurrentDirectory
    let mutable webappAssembly : string = null
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
        | Bin -> webappAssembly <- arg.Substring(3) //Path.Combine(curFolder, arg.Substring(3))
        | Target -> targetFolder <- arg.Substring(3) //Path.Combine(curFolder, arg.Substring(3))
        | _ -> ignore()

    let mutable inError = false
    let prevColor = Console.ForegroundColor
    
    if webappAssembly == null || not (File.Exists(webappAssembly)) then
        Console.ForegroundColor <- ConsoleColor.DarkRed
        Console.Error.WriteLine webappAssembly
        Console.Error.WriteLine "Invalid web app assembly path. Specify a assembly with -b:<folder>. Ex: -b:bin\web.dll"
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
        Console.WriteLine " mrtypegen.exe -b:<assembly path> -t:<gen files folder>"
        Console.WriteLine ""
        Console.WriteLine "Example:"
        Console.WriteLine " mrtypegen.exe -b:WebApp\bin\web.dll -t:WebApp\Generated"
        Console.WriteLine ""
        Environment.Exit -1

    let resolve_asm (sender) (args:ResolveEventArgs) : Assembly = 
        let asmName = AssemblyName(args.Name)
        try
            Assembly.Load asmName
        with 
        | exc -> 
            Console.WriteLine (sprintf "Could not load assembly %O. Tried from %s but got %O" args.Name asmName.Name exc)
            null
    
//    let _asmResolveHandler = ResolveEventHandler(resolve_asm)
//    let domain = AppDomain.CurrentDomain
//    domain.add_AssemblyResolve _asmResolveHandler

    generate_routes webappAssembly targetFolder

//    Console.ReadKey() |> ignore