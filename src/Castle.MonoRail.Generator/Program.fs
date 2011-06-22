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

    generate_routes binFolder targetFolder