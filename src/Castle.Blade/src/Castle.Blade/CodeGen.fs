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
    open System.Collections.Generic

    type CodeGenOptions (renderMethod:string, writeLiteral:string, write:string) = 
        let _imports = List<string>()
        let mutable _renderMethod = renderMethod
        let mutable _writeLiteral = writeLiteral
        let mutable _write = write
        let mutable _defNamespace = "Blade"
        let mutable _defBaseClass = "Castle.Blade.BaseBladePage"

        new () = CodeGenOptions("RenderPage", "WriteLiteral", "Write")

        member x.Imports = _imports

        member x.DefaultNamespace
            with get() = _defNamespace and set v = _defNamespace <- v
        
        member x.DefaultBaseClass
            with get() = _defBaseClass and set v = _defBaseClass <- v

        member x.RenderMethodName
            with get() = _renderMethod and set v = _renderMethod <- v

        member x.WriteLiteralMethodName  
            with get() = _writeLiteral and set v = _writeLiteral <- v

        member x.WriteMethodName  
            with get() = _write and set v = _write <- v


    module CodeGen = 

        open AST
        open System
        open System.IO
        open System.CodeDom
        open System.Collections.Generic
        open System.Text
        open System.CodeDom.Compiler
        open Microsoft.CSharp
        open Castle.Blade

        type internal StmtCollWrapper(addStmt:CodeStatement -> unit) =
            class
                let _buffer = StringBuilder()

                member x.AddLine(content:string) = 
                    let lines = content.Split ([|System.Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)
                    for line in lines do
                        _buffer.Append line |> ignore

                member x.AddExp (exp:CodeExpression) = 
                    x.Add (CodeExpressionStatement exp)
                
                member x.Add(stmt:CodeStatement) = 
                    x.Flush()
                    addStmt(stmt)

                member x.AddAll(stmts:CodeStatement seq) = 
                    x.Flush()
                    for s in stmts do
                        addStmt(s)

                member x.Flush() = 
                    if _buffer.ToString().Trim().Length <> 0 then 
                        let lines = _buffer.ToString().Split ([|System.Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)
                        for line in lines do 
                            addStmt(CodeSnippetStatement(line))
                        _buffer.Length <- 0

                static member FromMethod(cmethod:CodeMemberMethod) = 
                    StmtCollWrapper(fun s -> cmethod.Statements.Add s |> ignore)
                static member FromList(list:List<CodeStatement>) = 
                    StmtCollWrapper(fun s -> list.Add s)
                static member FromBuffer(buf:StringBuilder) = 
                    let provider = new CSharpCodeProvider()
                    let opts = new CodeGeneratorOptions ( IndentString = "    ", BracingStyle = "C" )
                    StmtCollWrapper(
                        fun s -> 
                            use writer = new StringWriter()
                            provider.GenerateCodeFromStatement(s, writer, opts)
                            buf.Append (writer.GetStringBuilder().ToString()) |> ignore
                    )
            end

        let build_compilation_unit (typeDecl:CodeTypeDeclaration) (options:CodeGenOptions) = 
            let compileUnit = CodeCompileUnit()
            
            let rootNs = CodeNamespace(options.DefaultNamespace)
            compileUnit.Namespaces.Add rootNs |> ignore

            for name in options.Imports do
                rootNs.Imports.Add (CodeNamespaceImport name) |> ignore
            
            typeDecl.BaseTypes.Add (CodeTypeReference options.DefaultBaseClass) |> ignore

            rootNs.Types.Add typeDecl |> ignore

            (compileUnit, rootNs)

        let internal textWriterName = "__writer"

        let internal writeLiteralContent (content:string) 
                                         (writeLiteralMethod:CodeMethodReferenceExpression) 
                                         (lambdaDepth:int) (stmtColl:StmtCollWrapper) =
            let args : CodeExpression seq = seq {
                            if lambdaDepth <> 0 then yield upcast CodeVariableReferenceExpression( textWriterName + lambdaDepth.ToString() )
                            yield upcast CodeSnippetExpression("\"" + content + "\"") 
                       }
            let writeContent = CodeMethodInvokeExpression(writeLiteralMethod, args |> Seq.toArray  )
            stmtColl.Add (CodeExpressionStatement writeContent)

        let internal writeCodeContent (codeExp:string) 
                                      (writeMethod:CodeMethodReferenceExpression) 
                                      (lambdaDepth:int) (stmtColl:StmtCollWrapper) =
            let args : CodeExpression seq = seq {
                            if lambdaDepth <> 0 then yield upcast CodeSnippetExpression( textWriterName + lambdaDepth.ToString() )
                            yield upcast CodeSnippetExpression(codeExp) 
                       }
            let writeContent = CodeMethodInvokeExpression(writeMethod, args |> Seq.toArray  )
            stmtColl.Add (CodeExpressionStatement writeContent)

        let rec internal gen_code (node:ASTNode) (rootNs:CodeNamespace) (typeDecl:CodeTypeDeclaration) (compUnit:CodeCompileUnit)  
                                    (stmtColl:StmtCollWrapper) 
                                    (writeLiteralMethod:CodeMethodReferenceExpression) (writeMethod:CodeMethodReferenceExpression)
                                    (withinCode:bool) (lambdaDepth:int) = 

            match node with
            | Markup content -> // of string
                writeLiteralContent content writeLiteralMethod lambdaDepth stmtColl 

            | MarkupBlock lst -> // of ASTNode list
                for n in lst do
                    gen_code n rootNs typeDecl compUnit (stmtColl) writeLiteralMethod writeMethod false lambdaDepth
            
            | MarkupWithinElement (tagNode, nd) -> // of string * ASTNode

                let tagNodesRevised = 
                    let markupNodes = 
                        tagNode |> 
                            function 
                            | MarkupBlock nodes -> nodes
                            | _ -> failwith "Expecting MarkupBlock"
                    let tagName = 
                        List.head markupNodes |> 
                            function 
                            | Markup c -> c 
                            | _ -> failwith "Expecting Markup node"
                    if tagName = "<text" then // text element needs to be stripped out from the list
                        // remove head and tail
                        // [(markupNodes[1])..(markupNodes[markupNodes.Length - 2])]
                        let newList =   
                            if markupNodes.Length = 2 then
                                // empty huh? 
                                [Markup("")]
                            elif markupNodes.Length = 3 then
                                [ (List.nth markupNodes 1) ]
                            else 
                                (seq { for item in 1..(markupNodes.Length - 2) do yield markupNodes.[item]; } |> Seq.toList )
                        MarkupBlock( newList )
                    else
                        MarkupBlock(markupNodes)


                let stmtlist = List<_>()
                let stmts = StmtCollWrapper.FromList stmtlist
                gen_code tagNodesRevised rootNs typeDecl compUnit stmts writeLiteralMethod writeMethod true lambdaDepth
                gen_code nd rootNs typeDecl compUnit stmts writeLiteralMethod writeMethod true lambdaDepth
                stmts.Flush()

                (*
                if tag <> "<text>" then
                    let elemName = 
                        let last = tag.IndexOfAny [|' ';'>'|]
                        tag.Substring (1, last - 1)
                        
                    writeLiteralContent tag writeLiteralMethod lambdaDepth stmtColl 
                    stmtColl.AddAll stmtlist
                    writeLiteralContent ("</" + elemName + ">") writeLiteralMethod lambdaDepth stmtColl 
                else
                *)
                stmtColl.AddAll stmtlist

            | Code codeContent -> // of string
                if withinCode then
                    stmtColl.AddLine codeContent
                else
                    writeCodeContent codeContent writeMethod lambdaDepth stmtColl
            
            | CodeBlock lst -> // of ASTNode list
                for n in lst do
                    gen_code n rootNs typeDecl compUnit (stmtColl) writeLiteralMethod writeMethod true lambdaDepth
        
            | Lambda (paramnames, nd) -> // of ASTNode
                stmtColl.AddLine (sprintf "%s => new HtmlResult(%s%d => { \r\n" (List.head paramnames) textWriterName (lambdaDepth + 1))
                //let templateWriterMethod = CodeMethodReferenceExpression(CodeVariableReferenceExpression(textWriterName), "Write")
                gen_code nd rootNs typeDecl compUnit stmtColl writeLiteralMethod writeMethod true (lambdaDepth + 1)
                stmtColl.AddLine "})"
        
            | Invocation (left, nd) -> // of string * ASTNode
                let rec fold_suffix (stmts:StmtCollWrapper) (lst:ASTNode list) = 
                    for s in lst do
                        match s with
                        | Invocation (name,next) -> 
                            stmts.AddLine "."
                            stmts.AddLine name
                            if next.IsSome then
                                fold_suffix stmts [next.Value]
                        | Param sndlst ->
                            stmts.AddLine "("
                            for n in sndlst do
                                gen_code n rootNs typeDecl compUnit stmts writeLiteralMethod writeMethod true lambdaDepth
                            stmts.AddLine ")"
                        | Code c ->
                            // buf.Append(c) |> ignore
                            stmts.AddLine c
                        | None | Comment -> ()
                        | _ ->  failwithf "which node is that? %O" (s.GetType())

                // we should fold suffixlst
                let buf = StringBuilder()
                let newstmts = StmtCollWrapper.FromBuffer buf
                newstmts.AddLine left
                if nd.IsSome then fold_suffix newstmts [nd.Value]
                newstmts.Flush()
                writeCodeContent (buf.ToString()) writeMethod lambdaDepth stmtColl

            | IfElseBlock (cond, trueBlock, otherBlock) -> // of ASTNode * ASTNode * ASTNode option
                let condition = match cond with | Code p -> p | _ -> failwith "Expecting Code node as an if condition"
                let trueStmtsList = List<CodeStatement>()
                let falseStmtsList = List<CodeStatement>()
                let trueStmts = StmtCollWrapper.FromList trueStmtsList
            
                gen_code trueBlock rootNs typeDecl compUnit trueStmts writeLiteralMethod writeMethod true lambdaDepth
                trueStmts.Flush()
            
                if (otherBlock.IsSome) then
                    let falseStmts = StmtCollWrapper.FromList falseStmtsList
                    gen_code otherBlock.Value rootNs typeDecl compUnit falseStmts writeLiteralMethod writeMethod true lambdaDepth
                    falseStmts.Flush()

                let ifStmt = CodeConditionStatement(CodeSnippetExpression(condition), trueStmtsList.ToArray(), falseStmtsList.ToArray())
                stmtColl.Add ifStmt

            | Param lst -> // of ASTNode list
                failwith "should not bump into Param in gen_code"
        
            | KeywordBlock (id, name, block) -> // of string * ASTNode
                if id = "section" then
                    // DefineSection ("test", () => { });
                    let buf = StringBuilder()
                    let block_stmts = StmtCollWrapper.FromBuffer buf
                    gen_code block rootNs typeDecl compUnit block_stmts writeLiteralMethod writeMethod true lambdaDepth
                    block_stmts.Flush()
                    stmtColl.Add (CodeSnippetStatement (sprintf "this.DefineSection(\"%s\", () => { %s });" name (buf.ToString()) ))

            | KeywordConditionalBlock (keyname, cond, block) -> // of string * ASTNode * ASTNode
                ()

            | ImportNamespaceDirective ns ->
                rootNs.Imports.Add (CodeNamespaceImport ns) |> ignore

            | FunctionsBlock code ->
                typeDecl.Members.Add (CodeSnippetTypeMember(code)) |> ignore

            | InheritDirective baseClass -> // of string
                typeDecl.BaseTypes.Clear()
                typeDecl.BaseTypes.Add (CodeTypeReference(baseClass)) |> ignore

            | ModelDirective modelType -> // of string
                let baseType = typeDecl.BaseTypes.[0]
                baseType.TypeArguments.Clear()
                baseType.TypeArguments.Add (CodeTypeReference(modelType)) |> ignore

            | HelperDecl (name, args, block) -> // of string * ASTNode * ASTNode
                // public Castle.Blade.Web.HtmlResult testing () 
                // {
                //     return new Castle.Blade.Web.HtmlResult(_writer => {
                //         WriteLiteralTo(_writer, "    <b>test test test</b>\r\n");
                //     });
                // }
                let buf = StringBuilder()
                match args with 
                | Code c ->
                    buf.Append "(" |> ignore
                    buf.Append c  |> ignore
                    buf.Append ")" |> ignore
                | Comment -> ()
                | _ -> failwithf "Unexpected node in helper declaration %O" args
                
                let blockbuf = StringBuilder()
                let block_stmts = StmtCollWrapper.FromBuffer blockbuf
                gen_code block rootNs typeDecl compUnit block_stmts writeLiteralMethod writeMethod true (lambdaDepth + 1)
                block_stmts.Flush()

                typeDecl.Members.Add 
                    (CodeSnippetTypeMember(
                        sprintf "public HtmlResult %s %O { \r\n\treturn new HtmlResult(%s%d => { \r\n%O }); }" name buf textWriterName (lambdaDepth + 1) blockbuf  )) |> ignore

            | TryStmt (block,catches,final) -> // of ASTNode * ASTNode list option * ASTNode option 
                ()

            | DoWhileStmt (block, cond) -> // of ASTNode * ASTNode
                ()

            | _ -> () // Comment / None 


        and GenerateCodeFromAST (typeName:string) (nodes:ASTNode seq) (options:CodeGenOptions) = 
            
            let typeDecl = CodeTypeDeclaration(typeName, IsClass=true)
            let compileUnit, rootNs = build_compilation_unit typeDecl options

            let _execMethod = CodeMemberMethod( Name = options.RenderMethodName, Attributes = (MemberAttributes.Override ||| MemberAttributes.Public) )
            // _execMethod.Parameters.Add (CodeParameterDeclarationExpression(typeof<System.IO.TextWriter>, "_writer")) |> ignore 
            
            let writeLiteralMethod = CodeMethodReferenceExpression(null, options.WriteLiteralMethodName)
            let writeMethod = CodeMethodReferenceExpression(null, options.WriteMethodName)
            
            typeDecl.Members.Add _execMethod |> ignore

            let stmts = StmtCollWrapper.FromMethod _execMethod
        
            for n in nodes do
                gen_code n rootNs typeDecl compileUnit stmts writeLiteralMethod writeMethod false 0

            stmts.Flush()

            compileUnit



