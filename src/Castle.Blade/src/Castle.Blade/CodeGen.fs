namespace Castle.Blade

module CodeGen = 

    open AST
    open System
    open System.IO
    open System.CodeDom
    open System.Collections.Generic
    open System.Text
    open System.CodeDom.Compiler
    open Microsoft.CSharp


    type internal StmtCollWrapper(addStmt:CodeStatement -> unit) =
        class
            let _buffer = StringBuilder()

            member x.AddLine(content:string) = 
                // if content.IndexOf(System.Environment.NewLine) <> -1 then
                let lines = content.Split ([|System.Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)
                for line in lines do
                    _buffer.Append line |> ignore
                // else
                //    _buffer.Append content |> ignore
                
            member x.Add(stmt:CodeStatement) = 
                x.Flush()
                addStmt(stmt)

            member x.AddAll(stmts:CodeStatement seq) = 
                x.Flush()
                for s in stmts do
                    addStmt(s)

            member x.Flush() = 
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
                let opts = new CodeGeneratorOptions ( IndentString = " ", BracingStyle = "C" )
                StmtCollWrapper(
                    fun s -> 
                        use writer = StringWriter()
                        provider.GenerateCodeFromStatement(s, writer, opts)
                        buf.Append (writer.GetStringBuilder().ToString()) |> ignore
                )
        end


    let rec internal gen_code (node:ASTNode) (typeDecl:CodeTypeDeclaration) 
                                (stmtColl:StmtCollWrapper) (writerMethod:CodeMethodReferenceExpression) 
                                (withinCode:bool) = 
        match node with
        | Markup content -> // of string
            let writeContent = CodeMethodInvokeExpression(writerMethod, CodeSnippetExpression("\"" + content + "\""))
            stmtColl.Add (CodeExpressionStatement writeContent)

        | MarkupBlock lst -> // of ASTNode list
            for n in lst do
                gen_code n typeDecl (stmtColl) writerMethod false
            
        | MarkupWithinElement (elemName, nd) -> // of string * ASTNode
            let stmtlist = List<_>()
            let stmts = StmtCollWrapper.FromList stmtlist
            gen_code nd typeDecl stmts writerMethod true
            stmts.Flush()

            if elemName <> "<text>" then
                let writeStart = CodeMethodInvokeExpression(writerMethod, CodeSnippetExpression("\"" + elemName + "\""))
                stmtColl.Add (CodeExpressionStatement writeStart)
                stmtColl.AddAll stmtlist
                let writeEnd = CodeMethodInvokeExpression(writerMethod, CodeSnippetExpression("\"</" + (elemName.Substring(1)) + "\""))
                stmtColl.Add (CodeExpressionStatement writeEnd)
            else
                stmtColl.AddAll stmtlist

        | Code codeContent -> // of string
            
            if withinCode then
                stmtColl.AddLine codeContent
            else
                let snippet = CodeSnippetExpression(codeContent)
                let writeContent = CodeMethodInvokeExpression(writerMethod, snippet)
                stmtColl.Add (CodeExpressionStatement writeContent)
            
        | CodeBlock lst -> // of ASTNode list
            for n in lst do
                gen_code n typeDecl (stmtColl) writerMethod true
        
        | Lambda (paramnames, nd) -> // of ASTNode
            stmtColl.AddLine (sprintf "item => new System.Web.WebPages.HelperResult(_template_writer => { \r\n" )
            let templateWriterMethod = CodeMethodReferenceExpression(CodeVariableReferenceExpression("_template_writer"), "Write")
            gen_code nd typeDecl stmtColl templateWriterMethod true
            stmtColl.AddLine "})"
        
        | Invocation (left, nd) -> // of string * ASTNode
            // we should fold suffixlst
            let rec fold_suffix (buf:StringBuilder) (lst:ASTNode list) = 
                for s in lst do
                    match s with
                    | Invocation (name,next) -> 
                        buf.Append(".") |> ignore
                        buf.Append(name) |> ignore
                        fold_suffix buf [next]
                    | Param sndlst ->
                        buf.Append("(") |> ignore
                        let newBuffer = StringBuilder()
                        let newstmts = StmtCollWrapper.FromBuffer newBuffer
                        for n in sndlst do
                            gen_code n typeDecl newstmts writerMethod true
                        newstmts.Flush()
                        buf.Append (newBuffer.ToString()) |> ignore
                        buf.Append(")") |> ignore

                    | Code c ->
                        buf.Append(c) |> ignore
                    | None | Comment -> ()
                    | _ ->  failwithf "which node is that? %O" (s.GetType())
            
            let buf = StringBuilder()
            fold_suffix buf [nd]
            let rest = left + (buf.ToString())

            let snippet = CodeSnippetExpression(rest)
            let writeContent = CodeMethodInvokeExpression(writerMethod, snippet)
            stmtColl.Add (CodeExpressionStatement writeContent)


        | IfElseBlock (cond, trueBlock, otherBlock) -> // of ASTNode * ASTNode * ASTNode option
            let condition = match cond with | Code p -> p | _ -> failwith "Expecting Code node as an if condition"
            let trueStmtsList = List<CodeStatement>()
            let falseStmtsList = List<CodeStatement>()
            let trueStmts = StmtCollWrapper.FromList trueStmtsList
            
            gen_code trueBlock typeDecl trueStmts writerMethod true
            trueStmts.Flush()
            
            if (otherBlock.IsSome) then
                let falseStmts = StmtCollWrapper.FromList falseStmtsList
                gen_code otherBlock.Value typeDecl falseStmts writerMethod true
                falseStmts.Flush()

            let ifStmt = CodeConditionStatement(CodeSnippetExpression(condition), trueStmtsList.ToArray(), falseStmtsList.ToArray())
            stmtColl.Add ifStmt

        | Param lst -> // of ASTNode list
            failwith "should not bump into Param in gen_code"
        
        | Member name -> // of string
            failwith "should not bump into Member in gen_code"
        
        | KeywordBlock (keyname, block) -> // of string * ASTNode
            ()

        | KeywordConditionalBlock (keyname, cond, block) -> // of string * ASTNode * ASTNode
            ()

        | _ -> () // Comment / None 


    and generate_code (nodes:ASTNode seq) = 
        let typeDecl = CodeTypeDeclaration("the_name")
        let _execMethod = CodeMemberMethod( Name = "_Execute" )
        _execMethod.Parameters.Add (CodeParameterDeclarationExpression(typeof<System.IO.TextWriter>, "_writer")) |> ignore 
        let writerMethod = CodeMethodReferenceExpression(CodeVariableReferenceExpression("_writer"), "Write")
    
        typeDecl.Members.Add _execMethod |> ignore
        let stmts = StmtCollWrapper.FromMethod _execMethod
        
        for n in nodes do
            gen_code n typeDecl stmts writerMethod false

        stmts.Flush()

        typeDecl



