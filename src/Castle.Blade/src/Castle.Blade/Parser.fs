namespace Castle.Blade

module Parser = 

    open FParsec
    open System
    open System.IO
    open System.Text
    open System.Collections.Generic
    open AST

    type UState = 
        { 
            isRoot : bool
            ElemStack : string list 
        }
        with
           static member Default = { isRoot = true; ElemStack = [] }

    let keywords = new Dictionary<string, CharStream<_> -> Reply<ASTNode>>()

    
    let ws    = spaces
    // let ws1   = spaces1
    // let str s = pstring s
    // let str_ws s = str s >>. ws
    let str s = pstring s .>> ws
    let pkeyword s = attempt (pstring s .>> notFollowedBy letter .>> notFollowedBy digit) .>> ws

    let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
        fun stream ->
            // System.Diagnostics.Debug.WriteLine (sprintf "%A: Entering %s" stream.Position label)
            let reply = p stream
            // System.Diagnostics.Debug.WriteLine (sprintf "%A: Leaving %s (%A) [%O]" stream.Position label reply.Status reply.Result)
            reply

    let ifParser, ifParserR = createParserForwardedToRef()
    let postfixParser, postfixParserR = createParserForwardedToRef()
    let transitionParser, transitionParserR = createParserForwardedToRef()
    let withinMarkupParser, withinMarkupParserR = createParserForwardedToRef()

    let userstate_pushElem elemName isRoot = 
        updateUserState (fun us -> {us with isRoot = isRoot; ElemStack = elemName :: us.ElemStack })
    
    let userstate_popElem  elemName isRoot = 
        updateUserState (fun us -> 
                                if us.isRoot <> isRoot then 
                                    failwithf "mixing roots for element being poped: %s" elemName
                                if us.ElemStack.Head <> elemName then 
                                    failwithf "attempt to pop different element: %s while on the stack we have %s" elemName us.ElemStack.Head
                                {us with isRoot = isRoot; ElemStack = List.tail us.ElemStack }                                    
                        )

    let markupblock pend (elemName:string) = 
        between (userstate_pushElem elemName true) (userstate_popElem elemName true) 
                    (many1Till withinMarkupParser pend) |>> MarkupBlock

    // <text> anything here is treated as text/content but can have @transitions <and> <tags> </text>
    let contentWithinElement (elName:string) = 
        // let startTag = "<" + elName + ">"
        let endTag = "</" + elName + ">"
        let prevChar = ref ' '
        let notTagEnd c = 
            if !prevChar = '>' then 
                false 
            else 
                prevChar := c
                true
        many1Satisfy notTagEnd
            .>>. markupblock (followedByString endTag) elName .>> str endTag 
                |>> MarkupWithinElement
            <!> "contentWithinElement"  

    // @( .. )
    // doesn't try to be smart. < or @ have no special meaning, therefore allowed
    let inParamTransitionExp = 
        let count = ref 0
        let codeonly = 
            function
            | '(' -> 
                incr count
                true
            | ')' -> 
                if !count = 0 then 
                    false
                else 
                    decr count
                    true
            | _ -> true
        str "(" >>. manySatisfy codeonly .>> str ")" |>> Code

    let peek_element_name (stream:CharStream<_>) (offset:int) = 
        let mutable innerCont = true
        let mutable count = offset
        while innerCont do
            let c = stream.Peek(count)
            if c = '>' || c = '/' || c = ' ' then
                innerCont <- false
            else
                count <- count + 1
        stream.PeekString(count).Substring(1)

    // everything is code except <aa (content) and @<aa (delegate)
    let rec_code (pstart:char) (pend:char) (allowInlineContent:bool) (stream:CharStream<_>) = 
        let mutable count = 0
        let mutable docontinue = true
        let sb = new StringBuilder()
        let stmts = List<ASTNode>()
        let mutable errorReply : Reply<_> = Unchecked.defaultof<_>
        while docontinue do
            if stream.Peek() = pstart then
                count <- count + 1
                sb.Append (stream.Read()) |> ignore
            elif stream.Peek() = pend then
                if count = 0 then
                    docontinue <- false
                else
                    count <- count - 1
                    sb.Append (stream.Read()) |> ignore
            
            elif allowInlineContent && stream.Peek() = '<' && isLetter (stream.Peek(1)) then
                if sb.Length <> 0 then 
                    stmts.Add (Code(sb.ToString()))
                    sb.Length <- 0
                
                let elemName = peek_element_name stream 1

                let reply = contentWithinElement elemName stream 
                if reply.Status <> ReplyStatus.Ok then
                    docontinue <- false
                    errorReply <- reply
                else
                    stmts.Add reply.Result

            elif stream.Peek() = '@' && stream.Peek(1) = '<' then
                if sb.Length <> 0 then 
                    stmts.Add (Code(sb.ToString()))
                    sb.Length <- 0
                
                stream.Read(1) |> ignore // consume @

                // build lambda node

                let elemName = peek_element_name stream 1

                let reply = contentWithinElement elemName stream
                if reply.Status <> ReplyStatus.Ok then
                    docontinue <- false
                    errorReply <- reply
                else
                    stmts.Add (Lambda (["item"], reply.Result))

            elif stream.PeekString(3) = "@=>" then
                // build lambda
                
                failwith "not supported yet"

            elif stream.Peek() = '@' then
                // transition
                if sb.Length <> 0 then 
                    stmts.Add (Code(sb.ToString()))
                    sb.Length <- 0
                let reply = transitionParser stream
                if reply.Status <> ReplyStatus.Ok then
                    docontinue <- false
                    errorReply <- reply
                else
                    stmts.Add reply.Result
            else 
                if stream.IsEndOfStream then 
                    docontinue <- false
                else
                    sb.Append(stream.Read()) |> ignore

        if errorReply <> Unchecked.defaultof<_> then
            errorReply
        else
            // if (stmts.Count = 0) then 
            //     Reply(Code(sb.ToString()))
            // else
            if sb.Length <> 0 then stmts.Add (Code(sb.ToString()))
            Reply(CodeBlock(stmts |> List.ofSeq))
                
    // { .. }
    let codeblock =
        // str "{" >>. ((fun stream -> readCodeBetween stream '{' '}' true) .>> str "}") 
        str "{" >>. (rec_code '{' '}' true) .>> str "}" 

    let content (stopAtEndElement:bool) = 
        // <b> something @ \r\n
        fun (stream:CharStream<_>) -> 
            let mutable cont = true
            let buffer = StringBuilder()
            let state : UState = (getUserState stream).Result
            let stopAtNewLine = 
                not (List.isEmpty state.ElemStack) && List.head state.ElemStack = "?"

            while cont do
                if stream.IsEndOfStream then 
                    cont <- false
                else
                    let c = stream.Peek()
                    match c with 
                    | '@' -> // stop
                        cont <- false
                    | '\r' | '\n' ->
                        if stopAtNewLine then
                            cont <- false
                    | '<' ->
                        if stopAtEndElement then 
                            
                            if not (List.isEmpty state.ElemStack) then
                                let elem = List.head state.ElemStack
                                let possibleStart = stream.PeekString (elem.Length + 2)
                                let possibleEnd = stream.PeekString (elem.Length + 3)
                                
                                if possibleStart = ("<" + elem + ">") || possibleStart = ("<" + elem + " ") then
                                    // starting, push another with isRoot = false
                                    userstate_pushElem elem false |> ignore
                                elif possibleEnd = ("</" + elem + ">") then
                                    // ending
                                    if (state.isRoot) then  // if is root, stop
                                        cont <- false
                                    else
                                        // if it's our, just pop
                                        userstate_popElem elem false |> ignore
                    | _ -> ()
                
                if cont then
                    buffer.Append (stream.Read()) |> ignore

            if buffer.Length <> 0 then
                Reply(Markup(buffer.ToString()))
            else 
                Reply(ReplyStatus.Error, ErrorMessageList(ErrorMessage.Expected("markup/literal expected")))
    
    // @:anything here is treated as text/content but can have @transitions <and> <tags>
    let textexp = 
        str ":" >>. markupblock followedByNewline "?" <!> "textexp"

    // @* ... *@
    let comments = 
        str "*" >>. 
                skipCharsTillString "*@" true Int32.MaxValue
                >>. preturn Comment <!> "comments"
    // @@
    let atexp = str "@" |>> Markup
    // somevalidname
    let identifier = (many1Satisfy (isNoneOf ". <?@({*")) <!> "id"
    // { }
    let suffixblock = 
        (ws >>. codeblock) <!> "suffixblock"
    // ( )
    let suffixparen = 
        (ws >>. (str "(" >>. (rec_code '(' ')' true) .>> str ")") ) 
        |>> (function
                | CodeBlock lst -> 
                    Param(lst)
                | Code c -> 
                    Param([Code c])
                | _ -> 
                    None
            )
        <!> "suffixparen"
    // @a.b
    let memberCall = 
        str "." >>. identifier .>>. (opt postfixParser)
        |>> (fun (a,b) -> 
                        match b with 
                        | Some v -> Invocation(a,v) 
                        | _ -> Member(a) ) 
                        <!> "memberCall"

    let postfixes = choice [ attempt memberCall; attempt suffixblock; attempt suffixparen;  ] <!> "postfixes"

    let idExpression = identifier .>>. (attempt postfixes <|>% ASTNode.None) |>> Invocation   <!> "idExpression"

    let blockkeywords = 
        fun (stream:CharStream<_>) -> 
                let state = stream.State
                let reply = identifier stream

                if reply.Status = Ok then
                    let possibleKeyword = reply.Result
                    let found, customParser = keywords.TryGetValue possibleKeyword
                    if not found then
                        stream.BacktrackTo state
                        Reply()
                    else 
                        customParser stream
                else
                    stream.BacktrackTo state
                    Reply()

    let transition = 
        str "@" >>. choice [ 
                                codeblock
                                inParamTransitionExp
                                textexp
                                blockkeywords
                                idExpression
                                comments 
                                atexp 
                           ]  <!> "transition"

    let terms = choice [ transition; content false ]

    let grammar  = many terms .>> eof

    // keyword (code) { block }
    let conditional_block s = 
        pipe2 inParamTransitionExp suffixblock 
            (fun a b -> KeywordConditionalBlock (s, a, b))  <!> ("conditional_block" + s)

    // keyword id { block }
    let identified_block s = 
        pipe2 (pkeyword s) suffixblock 
            (fun a b -> KeywordBlock (a, b))  <!> ("identified_block" + s)

    // if (paren) { block } [else ({ block } | rec if (paren)) ]
    let parse_if = 
        pipe3 inParamTransitionExp suffixblock (opt (str "else" >>. (attempt suffixblock <|> ifParser)))
            (fun paren block1 block2 -> IfElseBlock(paren, block1, block2))

    let inlineIf = pkeyword "if" >>. parse_if
    let withinMarkup = choice [ transition; content true ]

    do ifParserR            := inlineIf
    do postfixParserR       := postfixes
    do transitionParserR    := transition
    do withinMarkupParserR  := withinMarkup

    // C# specific
    keywords.["if"]         <- parse_if 
    keywords.["do"]         <- conditional_block "do" // ParseDoStatement
    keywords.["try"]        <- conditional_block "try" // ParseTryStatement
    keywords.["for"]        <- conditional_block "for" 
    keywords.["foreach"]    <- conditional_block "foreach"
    keywords.["while"]      <- conditional_block "while"
    keywords.["switch"]     <- conditional_block "switch"  // switch(exp) { case aa: {  break; } default: { break; } }
    keywords.["lock"]       <- conditional_block "lock"
    keywords.["using"]      <- conditional_block "using" // ParseUsingStatement using(new something) {    }
    keywords.["case"]       <- conditional_block "case"  // ParseCaseBlock
    keywords.["default"]    <- conditional_block "default" // ParseCaseBlock
    // razor keywords
    keywords.["section"]    <- identified_block "section"  // ParseSectionBlock
    keywords.["inherits"]   <- conditional_block "inherits" // ParseInheritsStatement
    keywords.["helper"]     <- conditional_block "helper"   // ParseHelperBlock
    keywords.["functions"]  <- conditional_block "functions"// ParseFunctionsBlock
    keywords.["namespace"]  <- conditional_block "namespace"// HandleReservedWord
    keywords.["class"]      <- conditional_block "class"    // HandleReservedWord
    keywords.["layout"]     <- conditional_block "layout"   // HandleReservedWord


    let parse_from_reader (reader:TextReader) (streamName:string) = 
        let content = reader.ReadToEnd()
        match runParserOnString grammar UState.Default streamName content with 
        | Success(result, _, _)   -> result :> ASTNode seq
        | Failure(errorMsg, _, _) -> failwith errorMsg

    let parse (content:string) = 
        match runParserOnString grammar UState.Default "" content with 
        | Success(result, _, _)   -> result :> ASTNode seq
        | Failure(errorMsg, _, _) -> failwith errorMsg

