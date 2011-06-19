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

    let inline escape_char (c:char) : bool * string = 
        match c with 
        | '\r' -> true, "\\r"
        | '\n' -> true, "\\n"
        | '\t' -> true, "\\t"
        | '"'  -> true, "\\\""
        | _ -> false, null

    let internal escape_string (str:string) = 
        let buf = new StringBuilder(str.Length)
        for c in str do
            match escape_char c with
            | true, s  -> buf.Append s |> ignore
            | false, _ -> buf.Append c |> ignore
        buf.ToString()

    let keywords = new Dictionary<string, CharStream<_> -> Reply<ASTNode>>()
    let ws    = spaces
    let str s = pstring s .>> ws
    let pkeyword s = attempt (pstring s .>> notFollowedBy letter .>> notFollowedBy digit) .>> ws

    let internal attempt2 (p: Parser<'a,'u>) : Parser<'a,'u> =
        fun stream ->
            // state is only declared mutable so it can be passed by ref, it won't be mutated
            let mutable state = CharStreamState(stream) // = stream.State (manually inlined)
            let mutable reply = p stream
            if reply.Status = Error then
                if state.Tag <> stream.StateTag then
                    reply.Error  <- nestedError stream reply.Error
                    reply.Status <- Error // turns FatalErrors into Errors
                    stream.BacktrackTo(&state) // passed by ref as a (slight) optimization
                //elif reply.Status = FatalError then
                //    reply.Status <- Error
            reply


    let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
        fun stream ->
            // System.Diagnostics.Debug.WriteLine (sprintf "%A: Entering %s" stream.Position label)
            let reply = p stream
            // System.Diagnostics.Debug.WriteLine (sprintf "%A: Leaving %s (%A) [%O]" stream.Position label reply.Status reply.Result)
            reply

    // somevalidname
    let identifier = (many1Satisfy (isNoneOf "\r\n,. <?@({*")) <!> "id"

    let ifParser, ifParserR = createParserForwardedToRef()
    let postfixParser, postfixParserR : Parser<ASTNode, UState> * Parser<ASTNode, UState> ref = createParserForwardedToRef()
    let transitionParser, transitionParserR = createParserForwardedToRef()
    let withinMarkupParser, withinMarkupParserR = createParserForwardedToRef()
    let suffixblockParser, suffixblockParserR = createParserForwardedToRef()

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
        <!> "markupblock " + elemName

    // <text> anything here is treated as text/content but can have @transitions <and> <tags> </text>
    let contentWithinElement (elName:string) = 
        let endTag = "</" + elName + ">"
        let prevChar = ref ' '
        let notTagEnd c = if !prevChar = '>' then false else prevChar := c; true
        pipe2 (many1Satisfy notTagEnd) // read the whole start tag as it may have attributes and such
            // (markupblock (followedByString endTag) elName .>> str endTag)
            // then read the content within the tag
            (fun s -> 
                let res = s |> markupblock (followedByString endTag) elName 
                if res.Status = ReplyStatus.Error then
                    Reply(ReplyStatus.FatalError, res.Error)
                else
                    s |> str endTag |> ignore // consume the end element
                    res
                ) 
                (fun tag node -> MarkupWithinElement(escape_string(tag), node))
            <!> "contentWithinElement " + elName

    // @( .. )
    // doesn't try to be smart. < or @ have no special meaning, therefore allowed
    let inParamTransitionExp = 
        let count = ref 0
        let codeonly = 
            function
            | '(' -> incr count; true
            | ')' -> if !count = 0 then false else decr count; true
            | _ -> true
        str "(" >>. manySatisfy codeonly .>> str ")" |>> Code
        <!> "inParamTransitionExp"

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
        let mutable inQuote = false
        let mutable lastChar = ' '

        while docontinue do
            if stream.Peek() = '"' && lastChar <> '\\' then
                inQuote <- not inQuote
                let ch = stream.Read()
                lastChar <- ch
                sb.Append ch |> ignore
            else 
                if inQuote then 
                    let ch = stream.Read()
                    lastChar <- ch
                    sb.Append ch |> ignore
                else
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
                        if sb.Length <> 0 then stmts.Add (Code(sb.ToString())); sb.Length <- 0
                
                        let elemName = peek_element_name stream 1

                        let reply = contentWithinElement elemName stream 
                        if reply.Status <> ReplyStatus.Ok then
                            docontinue <- false
                            errorReply <- reply
                        else
                            stmts.Add reply.Result

                    elif stream.Peek() = '@' && stream.Peek(1) = '<' then
                        if sb.Length <> 0 then stmts.Add (Code(sb.ToString())); sb.Length <- 0
                
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
                        if sb.Length <> 0 then stmts.Add (Code(sb.ToString())); sb.Length <- 0

                        // @=> identifier <el> ... </el>

                        stream.Read(3) |> ignore // consume @=>
                        let replyForId = stream |> (spaces >>. identifier .>> spaces) 
                        if replyForId.Status <> ReplyStatus.Ok then
                            docontinue <- false
                            errorReply <- Reply(ReplyStatus.Error, replyForId.Error)
                        else
                            if stream.Peek() = '{' then 
                                // process block
                                let reply = stream |> suffixblockParser
                                if reply.Status <> ReplyStatus.Ok then
                                    docontinue <- false
                                    errorReply <- reply
                                else
                                    stmts.Add (Lambda ([replyForId.Result], reply.Result))

                            elif stream.Peek() = '<' then
                                // process markup block 
                                let elemName = peek_element_name stream 1
                                let reply = contentWithinElement elemName stream
                                if reply.Status <> ReplyStatus.Ok then
                                    docontinue <- false
                                    errorReply <- reply
                                else
                                    stmts.Add (Lambda ([replyForId.Result], reply.Result))

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
                            let ch = stream.Read()
                            lastChar <- ch
                            sb.Append ch |> ignore

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
        str "{" >>. (rec_code '{' '}' true) .>> str "}" 
        <!> "codeblock"

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
                    let c = stream.Read()
                    match escape_char c with 
                    | true, s -> buffer.Append s |> ignore
                    | _       -> buffer.Append c |> ignore

            if buffer.Length <> 0 then
                Reply(Markup(buffer.ToString()))
            else 
                Reply(ReplyStatus.Error, ErrorMessageList(ErrorMessage.Expected("markup/literal expected")))
    
    // @:anything here is treated as text/content but can have @transitions <and> <tags>
    let textexp = 
        str ":" >>. markupblock followedByNewline "?" 
        <!> "textexp"

    // @* ... *@
    let comments = 
        str "*" >>. 
                skipCharsTillString "*@" true Int32.MaxValue
                >>. preturn Comment 
        <!> "comments"

    // @@
    let atexp = str "@" |>> Markup <!> "atexp"

    // { }
    let suffixblock = 
        (ws >>. codeblock) <!> "suffixblock"

    // ( )
    let suffixparen = 
        ((str "(" >>. (rec_code '(' ')' true) .>> str ")") ) 
        |>> (fun x -> match x with
                | CodeBlock lst -> 
                    Param(lst)
                | Code c -> 
                    Param([Code c])
                | _ -> 
                    failwithf "unexpected node %O"  x
            )
        <!> "suffixparen"

    // @a.b
    let memberCall = 
        str "." >>. identifier .>>. (opt (postfixParser))
        |>> Invocation <!> "memberCall"

    let postfixes = 
        choice [ attempt2 memberCall; attempt2 suffixblock; attempt2 suffixparen;  ] 
        <!> "postfixes"

    let idExpression = identifier .>>. (opt (postfixes)) |>> Invocation   <!> "idExpression"

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
        <!> "blockkeywords"

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

    let terms = choice [ transition; content false ] <!> "terms"

    let grammar  = many terms .>> eof


    let directiveCharPred = (noneOf "\r\n;{@")
    let directiveTerminator = (newline <|> (pchar ';'))
    let directiveParser = many1CharsTill directiveCharPred directiveTerminator

    // keyword (code) { block }
    let conditional_block s = 
        // changed inParamTransitionExp to suffixparen
        spaces >>. pipe2 suffixparen suffixblock 
            (fun a b -> KeywordConditionalBlock (s, a, b))  
        <!> ("conditional_block " + s)

    // keyword id { block }
    let identified_block s = 
        spaces >>. pipe2 (identifier) suffixblock 
            (fun id block -> KeywordBlock (s, id, block))  
        <!> ("identified_block " + s)

    // if (paren) { block } [else ({ block } | rec if (paren)) ]
    let parse_if = 
        pipe3 inParamTransitionExp suffixblock (opt (str "else" >>. (attempt suffixblock <|> ifParser)))
              (fun paren block1 block2 -> IfElseBlock(paren, block1, block2))
        <!> "parse_if"
    
    // using namespace or using(exp) { block }
    let parse_using = 
        (attempt (conditional_block "using") <|> ((directiveParser) |>> ImportNamespaceDirective))
        <!> "parse_using"

    // @helper name(args) { block }
    let parse_helper = 
        spaces >>. pipe4 identifier spaces inParamTransitionExp suffixblock 
                         (fun id _ args block -> HelperDecl(id, args, block))
        <!> "parse_helper"

    // @inherits This.Is.A.TypeName<Type>[;]
    let parse_inherits = 
        directiveParser |>> InheritDirective
        <!> "parse_inherits"

    // @model modelTypeName
    let parse_model = 
        directiveParser |>> ModelDirective
        <!> "parse_model"

    // try { } (many [ catch(ex) { } ]) ([ finally { } ])
    let parse_try = 
        spaces >>. 
            pipe3 suffixblock (opt (many (conditional_block "catch"))) (opt (spaces >>. str "finally" >>. suffixblock))
                (fun b catches final -> TryStmt(b, catches, final))
        <!> "parse_try"

    // do { block } while ( cond )
    let parse_do = 
        pipe4 suffixblock spaces (str "while") inParamTransitionExp
            (fun block _ _ cond -> DoWhileStmt(block, cond))
        <!> "parse_do"

    // fails parser
    let reserved_keyword name = 
        failFatally (sprintf "Invalid use of reserved keyword %s" name) 
        <!> "reserved_keyword"

    // @functions { }
    let parse_functions = 
        let count = ref 0
        let codeonly = function
                        | '{' -> incr count; true
                        | '}' -> if !count = 0 then false else decr count; true
                        | _ -> true
        spaces >>. pchar '{' >>. manySatisfy codeonly .>> pchar '}' |>> FunctionsBlock
        <!> "parse_functions"

    let inlineIf = pkeyword "if" >>. parse_if
    let withinMarkup = choice [ transition; content true ]

    do ifParserR            := inlineIf
    do postfixParserR       := postfixes
    do transitionParserR    := transition
    do withinMarkupParserR  := withinMarkup
    do suffixblockParserR   := suffixblock

    // C# specific
    keywords.["if"]         <- parse_if 
    keywords.["do"]         <- parse_do 
    keywords.["try"]        <- parse_try
    keywords.["for"]        <- conditional_block "for" 
    keywords.["foreach"]    <- conditional_block "foreach"
    keywords.["while"]      <- conditional_block "while"
    keywords.["switch"]     <- conditional_block "switch"  // switch(exp) { case aa: {  break; } default: { break; } }
    keywords.["lock"]       <- conditional_block "lock"
    keywords.["using"]      <- parse_using               
    keywords.["case"]       <- conditional_block "case"    // ParseCaseBlock
    keywords.["default"]    <- conditional_block "default" // ParseCaseBlock
    // razor keywords
    keywords.["section"]    <- identified_block "section"  
    keywords.["inherits"]   <- parse_inherits 
    keywords.["helper"]     <- parse_helper   
    keywords.["model"]      <- parse_model   
    keywords.["functions"]  <- parse_functions 
    keywords.["namespace"]  <- reserved_keyword "namespace" 
    keywords.["class"]      <- reserved_keyword "class"    
    keywords.["layout"]     <- reserved_keyword "layout"   

    let parse_from_reader (reader:TextReader) (streamName:string) = 
        let content = reader.ReadToEnd()
        match runParserOnString grammar UState.Default streamName content with 
        | Success(result, _, _)   -> result :> ASTNode seq
        | Failure(errorMsg, _, _) -> failwith errorMsg

    let parse_from_stream (stream:Stream) (streamName:string) (enc:Encoding) = 
        match runParserOnStream grammar UState.Default streamName stream enc with 
        | Success(result, _, _)   -> result :> ASTNode seq
        | Failure(errorMsg, _, _) -> failwith errorMsg
    
    let parse_string (content:string) = 
        match runParserOnString grammar UState.Default "" content with 
        | Success(result, _, _)   -> result :> ASTNode seq
        | Failure(errorMsg, _, _) -> failwith errorMsg

