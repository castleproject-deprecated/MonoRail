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

namespace Castle.Blade 

    open System
    open System.IO
    open System.Collections.Generic
    open System.Text
    

    type BladeEngineHost(opts:CodeGenOptions) = 
        let mutable _codeGenOptions = opts

        member x.CodeGenOptions 
            with get() = _codeGenOptions and set v = _codeGenOptions <- v

        member x.GenerateCode (reader:TextReader, generatedFileName:string, sourcefileName:string) = 
            let node = Parser.parse_from_reader reader sourcefileName
            let compilationUnit = CodeGen.GenerateCodeFromAST generatedFileName node _codeGenOptions
            compilationUnit

        member x.GenerateCode (stream:Stream, generatedFileName:string, sourcefileName:string, enc:Encoding) = 
            let node = Parser.parse_from_stream stream sourcefileName enc
            let compilationUnit = CodeGen.GenerateCodeFromAST generatedFileName node _codeGenOptions
            compilationUnit
        

