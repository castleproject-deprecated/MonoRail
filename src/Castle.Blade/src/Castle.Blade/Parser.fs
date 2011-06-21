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

    type StackElemType = 
        | None    = 0
        | InElem  = 1
        | Elem    = 2
        | Char    = 3
        | NewLine = 4

    type StackElem = { 
        IsRoot : bool
        ElemName : string
        EndElemTag : string
        BeginChar : char
        EndChar : char
        ElemType : StackElemType
    }
    with static member Default = { ElemType = StackElemType.None; 
                                   IsRoot = false; ElemName = null; EndElemTag = null; 
                                   BeginChar = ' '; EndChar = ' ' }
    type UState = { 
        ElemStack : StackElem list
    }
    with static member Default = { ElemStack = [] }

    let userstate_pushChar bchar echar isRoot =
        updateUserState (
            fun us -> 
            { us with ElemStack = { ElemType = StackElemType.Char; 
                                    IsRoot = isRoot; ElemName = null; EndElemTag = null; 
                                    BeginChar = bchar; EndChar = echar } :: us.ElemStack })
    let userstate_popChar bchar echar isRoot =
        updateUserState (
            fun us -> // todo: assert same
            { us with ElemStack = List.tail us.ElemStack })

    let userstate_pushInElement = 
        updateUserState (
            fun us -> 
            { us with ElemStack = { ElemType = StackElemType.InElem; 
                                    IsRoot = false; ElemName = null; EndElemTag = null; 
                                    BeginChar = ' '; EndChar = ' ' } :: us.ElemStack })
    let userstate_popInElement = 
        updateUserState (
            fun us -> // todo: assert same
            { us with ElemStack = List.tail us.ElemStack })

    let userstate_pushStopAtNewline = 
        updateUserState (
            fun us -> 
            { us with ElemStack = { ElemType = StackElemType.NewLine; 
                                    IsRoot = false; ElemName = null; EndElemTag = null;
                                    BeginChar = ' '; EndChar = ' ' } :: us.ElemStack })
    let userstate_popStopAtNewline = 
        updateUserState (
            fun us -> // todo: assert same
            { us with ElemStack = List.tail us.ElemStack })
    
    let userstate_pushElem elemName endTag isRoot = 
        updateUserState (
            fun us -> 
            { us with ElemStack = { ElemType = StackElemType.Elem;
                                    IsRoot = isRoot; ElemName = elemName; EndElemTag = endTag; 
                                    BeginChar = ' '; EndChar = ' ' } :: us.ElemStack })
    let userstate_popElem  elemName endTag isRoot = 
        updateUserState (
            fun us -> 
                let h = us.ElemStack.Head
                if h.IsRoot <> isRoot       then  failwithf "mixing roots for element being poped: %s" elemName
                if h.ElemName <> elemName   then  failwithf "attempt to pop different element: %s while on the stack we have %s" elemName h.ElemName
                if h.EndElemTag <> endTag   then  failwithf "mixing endTag for element being poped: %s" endTag
                { us with ElemStack = List.tail us.ElemStack } )

    let keywords = new Dictionary<string, CharStream<_> -> Reply<ASTNode>>()
    let transitionParser, transitionParserR = createParserForwardedToRef()
    let markupParser, markupParserR = createParserForwardedToRef()
    let codeblockParser, codeblockParserR = createParserForwardedToRef()

    let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
        fun stream ->
            System.Diagnostics.Debug.WriteLine (sprintf "%A: Entering %s" stream.Position label)
            let reply = p stream
            System.Diagnostics.Debug.WriteLine (sprintf "%A: Leaving %s (%A) [%O]" stream.Position label reply.Status reply.Result)
            reply

    // isNoneOf "=-:\r\n,. <>?@(){}'*"
    let identifier = (many1Satisfy (fun c -> isLetter(c) || isDigit(c)))  <!> "identifier"

    let peek_element_name (stream:CharStream<_>) (offset:int) = 
        let mutable innerCont = true
        let mutable count = offset
        let mutable isCommentElem = false
        // todo: add support for CDATA
        while innerCont do
            let c = stream.Peek(count)
            if c = '>' || c = '/' || c = ' ' || c = '!' then
                if c = '!' && stream.Peek(offset + 1) = '-' && stream.Peek(offset + 2) = '-' then 
                    isCommentElem <- true
                innerCont <- false
            else
                count <- count + 1
        if (isCommentElem) then
            stream.PeekString(offset + 3).Substring(1), "-->"
        else
            let el = stream.PeekString(count).Substring(1)
            el, "</" + el + ">"

    let inline escape_char (c:char) : bool * string = 
        match c with 
        | '\r' -> true, "\\r"
        | '\n' -> true, "\\n"
        | '\t' -> true, "\\t"
        | '"'  -> true, "\\\""
        | _ -> false, null

    // ----------------
    // parsers
    // ----------------

    // @* ... *@
    let comments = 
        pstring "*" >>. 
                skipCharsTillString "*@" true Int32.MaxValue
                >>. preturn Comment 
        <!> "comments"

    // @keyword -> delegate to specialized parser if found. otherwise, backtracks and returns
    let transitionToKeyword = 
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
        <!> "transitionToKeyword"

    // <element src="@SomeCall()"> <- parses the content of the tag from the end of the name, til the /> or >
    let markupblockwithinelementAttrs = 
        between (userstate_pushInElement) (userstate_popInElement) markupParser 
        <!> "markupblockwithinelementAttrs"

    // <text> anything here is treated as text/content but can have @transitions <and> <tags> </text>
    let contentWithinElement (elName:string) (endTag:string) = 
        between (userstate_pushElem elName endTag true) (userstate_popElem elName endTag true)
                (pipe2 (markupblockwithinelementAttrs)  // read the whole start tag as it may have attributes and such
                    // then read the content within the tag if any
                    (fun s -> 
                        let res = s |> markupParser  // (followedByString endTag) elName endTag
                        if res.Status = ReplyStatus.Error then
                            Reply(ReplyStatus.FatalError, res.Error)
                        else
                            res
                        ) 
                        (fun (tag:ASTNode) (node:ASTNode) -> 
                            if elName = "text" then
                                // MarkupWithinElement(List.tail tag, node |> List.choose ))
                                let mblock = node.ToList() |> List.choose (fun n -> if n.Content() <> "</text>" then Some(n) else None)
                                MarkupWithinElement(
                                    MarkupBlock(tag.ToList() |> List.tail), 
                                    MarkupBlock(mblock))
                            else
                                MarkupWithinElement(tag, node))
                        )
                    <!> "contentWithinElement " + elName

    let markupblock = 
        between (userstate_pushChar '{' '}' true) (userstate_popChar '{' '}' true)
                (markupParser) // .>> pstring "}"
        <!> "markupblock"

    // everything is code except <aa (content) and @<aa (delegate)
    let private rec_code (pstart:char) (pend:char) (allowInlineContent:bool) (stream:CharStream<_>) = 
        let sb = new StringBuilder()
        let stmts = List<ASTNode>()
        let docontinue = ref true
        let mutable acceptsMarkup = true
        let mutable inQuote = false
        let errorReply = ref Unchecked.defaultof<Reply<_>>
        let count = ref 1
        let lastChar = ref ' '

        let flush() = 
            if sb.Length <> 0 then stmts.Add (Code(sb.ToString())); 
            sb.Length <- 0
        
        let readchar() = 
            let ch = stream.Read()
            lastChar := ch; sb.Append ch |> ignore

        let build_lambda() = 
            let elemNameStart, eleEnd = peek_element_name stream 1
            let reply = contentWithinElement elemNameStart eleEnd stream
            if reply.Status <> ReplyStatus.Ok then
                docontinue := false
                errorReply := reply
            else
                stmts.Add (Lambda (["item"], reply.Result))

        let build_named_lambda() = 
            stream.Read(3) |> ignore // consume @=>
            let replyForId = stream |> (spaces >>. identifier .>> spaces) 
            if replyForId.Status <> ReplyStatus.Ok then
                docontinue := false
                errorReply := Reply(ReplyStatus.Error, replyForId.Error)
            else
                if stream.Peek() = '{' then 
                    // process block
                    let reply = stream |> codeblockParser
                    if reply.Status <> ReplyStatus.Ok then
                        docontinue := false
                        errorReply := reply
                    else
                        stmts.Add (Lambda ([replyForId.Result], reply.Result))
                elif stream.Peek() = '<' then
                    // process markup block 
                    let elemName, eleEnd = peek_element_name stream 1
                    let reply = contentWithinElement elemName eleEnd stream
                    if reply.Status <> ReplyStatus.Ok then
                        docontinue := false
                        errorReply := reply
                    else
                        stmts.Add (Lambda ([replyForId.Result], reply.Result))            

        while !docontinue && not stream.IsEndOfStream do
            let c = stream.Peek()
            if c = '"' && !lastChar <> '\\' then
                inQuote <- not inQuote
                readchar()
            else
                if inQuote then 
                    readchar()
                else 
                    if c = pstart then
                        incr count; sb.Append (stream.Read()) |> ignore
                    elif c = pend then
                        decr count
                        if !count = 0 then
                            docontinue := false
                        else
                            sb.Append (stream.Read()) |> ignore
                    elif c = '\r' && stream.Peek(1) = '\n' then
                        acceptsMarkup <- true
                        readchar(); readchar()
                    elif c = ';' then
                        acceptsMarkup <- true
                        readchar()
                    elif allowInlineContent && acceptsMarkup && c = '<' && (isLetter (stream.Peek(1)) || stream.PeekString(4) = "<!--") then
                        flush()
                        
                        let elemNameStart, eleEnd = peek_element_name stream 1
                        let reply = contentWithinElement elemNameStart eleEnd stream 
                        if reply.Status <> ReplyStatus.Ok then
                            docontinue := false
                            errorReply := reply
                        else
                            stmts.Add reply.Result

                    elif c = '@' && stream.Peek(1) = '<' then
                        flush()
                        build_lambda()
                    elif stream.PeekString(3) = "@=>" then
                        flush()
                        build_named_lambda()
                    elif c = '@' then
                        flush()
                        let reply = transitionParser stream
                        if reply.Status <> ReplyStatus.Ok then
                            docontinue := false
                            errorReply := reply
                        else
                            stmts.Add reply.Result
                    else
                        if c <> ' ' && c <> '\t' then
                            acceptsMarkup <- false
                        readchar() 

        if !errorReply <> Unchecked.defaultof<_> then
            !errorReply
        else
            flush()
            Reply(CodeBlock(stmts |> List.ofSeq))

    let private codeblock = 
        pstring "{" >>. (rec_code '{' '}' true) .>> pstring "}" 
        <!> "codeblock"

    do codeblockParserR := codeblock

    let codeblock_s = spaces >>. codeblock


    let markup = 
        fun (stream:CharStream<_>) -> 
            let buffer = StringBuilder()
            let nodes = List<ASTNode>()
            let state = 
                let tmp = (getUserState stream).Result
                if tmp.ElemStack.IsEmpty then StackElem.Default else tmp.ElemStack.Head
            let startEndCharDepth = ref 0
            let mutable skipChar = false
            let mutable contLoop = true
            let mutable freply = lazy ( Reply(MarkupBlock( nodes.ToArray() |> Array.toList )) )

            while contLoop && not stream.IsEndOfStream do
                let c = stream.Peek()
                match c with
                | '\r' | '\n' ->
                        if state.ElemType = StackElemType.NewLine then contLoop <- false
                | '/' ->
                    if state.ElemType = StackElemType.InElem && stream.Peek(1) = '>' then
                        buffer.Append (stream.Read(2)) |> ignore; contLoop <- false
                | '>' ->
                    if state.ElemType = StackElemType.InElem then
                        buffer.Append (stream.Read(1)) |> ignore; contLoop <- false
                | '<' | '-' -> 
                    // need to consider - as it ends comments (-->)
                    if state.ElemType = StackElemType.Elem then
                        let elemName = state.ElemName
                        let elemTagEnd = state.EndElemTag
                        let possibleStart = stream.PeekString (elemName.Length + 2)
                        let possibleEnd = stream.PeekString (elemTagEnd.Length)
                                
                        if possibleStart = ("<" + elemName + ">") || possibleStart = ("<" + elemName + " ") then
                            // starting a new element with same name, push another with isRoot = false
                            userstate_pushElem elemName elemTagEnd false |> ignore
                        elif possibleEnd = elemTagEnd then
                            // ending
                            if (state.IsRoot) then  // if is root, stop
                                contLoop <- false
                                buffer.Append (stream.Read(elemTagEnd.Length)) |> ignore
                            else
                                // if it's ours, just pop
                                userstate_popElem elemName elemTagEnd false |> ignore
                | '@' ->
                    if stream.Peek(1) = '@' then
                        stream.Read 2 |> ignore
                        buffer.Append '@' |> ignore
                    else
                        nodes.Add (Markup (buffer.ToString()))
                        buffer.Length <- 0
                        let reply = transitionParser stream
                        if reply.Status = Ok then
                            nodes.Add reply.Result
                            skipChar <- true
                        else 
                            freply <- lazy reply
                            contLoop <- false
                | _ -> 
                    if state.ElemType = StackElemType.Char then
                        if c = state.BeginChar then
                            incr startEndCharDepth 
                        elif c = state.EndChar then
                            decr startEndCharDepth
                            if !startEndCharDepth = 0 then
                                contLoop <- false
                                buffer.Append (stream.Read(1)) |> ignore
                
                if not skipChar && contLoop then
                    let c = stream.Read()
                    match escape_char c with 
                    | true, s -> buffer.Append s |> ignore
                    | _       -> buffer.Append c |> ignore
                else
                    // reset flag 
                    skipChar <- false

            if buffer.Length <> 0 then 
                nodes.Add (Markup(buffer.ToString()))

            freply.Force()

    // @( .. )
    // doesn't try to be smart. < or @ have no special meaning, therefore allowed
    let parenthesesCodeOnly = 
        let count = ref 0
        let codeonly = 
            function
            | '(' -> incr count; true
            | ')' -> if !count = 0 then false else decr count; true
            | _ -> true
        pstring "(" >>. manySatisfy codeonly .>> pstring ")" |>> Code
        <!> "parenthesesCodeOnly"

    // ( allows transitions here )
    let parenthesesAllowingTransition = 
        pstring "(" >>. (rec_code '(' ')' true) .>> pstring ")"  
        |>> (fun x -> 
                match x with
                | CodeBlock lst -> Param(lst)
                | Code c -> Param([Code c])
                | _ -> failwithf "unexpected node %O"  x
            )
        <!> "parenthesesAllowingTransition"

    let postfixParser, postfixParserR = createParserForwardedToRef()
    
    // .MemberName [ .identifier | (args) ]
    let memberAccess = 
        pstring "." >>. identifier .>>. (opt (postfixParser))
        |>> Invocation <!> "memberAccess"

    // [ .identifier | (args) ]
    let postfixes = 
        choice [ attempt memberAccess; attempt parenthesesAllowingTransition;  ] 
        <!> "postfixes"
    do postfixParserR := postfixes

    // @identifier[ .identifier | (args) ]
    let idExpression = 
        identifier .>>. (opt (postfixes)) 
        |>> Invocation   <!> "idExpression"

    // @:anything here is treated as text/content but can have @transitions <and> <tags>
    let colonTransitionToMarkup = 
        between (userstate_pushStopAtNewline) (userstate_popStopAtNewline) 
                (pstring ":" >>. markupParser) 
        <!> "colonTransitionToMarkup"

    // @@ -> Markup of "@"
    let doubleAt = pstring "@" |>> Markup <!> "doubleAt"

    let private transition = 
        pstring "@" >>. choice  [ 
                                    codeblock
                                    parenthesesCodeOnly
                                    transitionToKeyword
                                    idExpression
                                    colonTransitionToMarkup
                                    comments 
                                    doubleAt
                                ]  <!> "transition"

    do transitionParserR := transition
    do markupParserR := markup

    let grammar  = markup .>> eof

    // fails parser
    let reserved_keyword name = 
        failFatally (sprintf "Invalid use of reserved keyword %s" name) 
        <!> "reserved_keyword"

    let directiveCharPred = (noneOf "\r\n;{@")
    let directiveTerminator = (newline <|> (pchar ';'))
    let directiveParser = many1CharsTill directiveCharPred directiveTerminator
    
    // keyword (code allowing transitions) { block }
    let conditional_block_t s = 
        // changed inParamTransitionExp to suffixparen
        spaces >>. pipe2 parenthesesAllowingTransition codeblock_s 
            (fun a b -> KeywordConditionalBlock (s, a, b))  
        <!> ("conditional_block " + s)
    // keyword (code) { block }
    let conditional_block s = 
        spaces >>. pipe2 parenthesesCodeOnly codeblock_s 
            (fun a b -> KeywordConditionalBlock (s, a, b))  
        <!> ("conditional_block " + s)

    // keyword id { block }
    let identified_block s = 
        spaces >>. pipe2 (identifier) codeblock_s 
            (fun id block -> KeywordBlock (s, id, block))  
        <!> ("identified_block " + s)

    // @section name { markup }
    let parse_section = 
        spaces >>. pipe2 identifier markupblock
            (fun a b -> KeywordBlock ("section", a, b))  
        <!> ("parse_section")

    let ifParser, ifParserR = createParserForwardedToRef()

    // if (paren) { block } [else ({ block } | rec if (paren)) ]
    let parse_if = 
        pipe3 parenthesesAllowingTransition codeblock_s 
                (opt (attempt (spaces >>. pstring "else" >>. (attempt codeblock_s <|> ifParser))))
              (fun paren block1 block2 -> IfElseBlock(paren, block1, block2))
        <!> "parse_if"
    
    let inlineIf = spaces >>. pstring "if" >>. spaces >>. parse_if
    do ifParserR := inlineIf

    // using namespace or using(exp) { block }
    let parse_using = 
        (attempt (conditional_block "using") <|> ((directiveParser) |>> ImportNamespaceDirective))
        <!> "parse_using"

    // @helper name(args) { block }
    let parse_helper = 
        spaces >>. pipe4 identifier spaces parenthesesCodeOnly codeblock_s
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
            pipe3 codeblock_s (opt (many (conditional_block "catch"))) (opt (spaces >>. pstring "finally" >>. codeblock_s))
                (fun b catches final -> TryStmt(b, catches, final))
        <!> "parse_try"

    // do { block } while ( cond )
    let parse_do = 
        pipe4 codeblock_s spaces (pstring "while") parenthesesAllowingTransition
            (fun block _ _ cond -> DoWhileStmt(block, cond))
        <!> "parse_do"

    // @functions { }
    let parse_functions = 
        let count = ref 0
        let codeonly = function
                        | '{' -> incr count; true
                        | '}' -> if !count = 0 then false else decr count; true
                        | _ -> true
        spaces >>. pchar '{' >>. manySatisfy codeonly .>> pchar '}' |>> FunctionsBlock
        <!> "parse_functions"

    // C# specific
    keywords.["if"]         <- parse_if 
    keywords.["do"]         <- parse_do 
    keywords.["try"]        <- parse_try
    keywords.["for"]        <- conditional_block_t "for" 
    keywords.["foreach"]    <- conditional_block_t "foreach"
    keywords.["while"]      <- conditional_block_t "while"
    keywords.["switch"]     <- conditional_block "switch"  // switch(exp) { case aa: {  break; } default: { break; } }
    keywords.["lock"]       <- conditional_block "lock"
    keywords.["using"]      <- parse_using               
    keywords.["case"]       <- conditional_block "case"    // ParseCaseBlock
    keywords.["default"]    <- conditional_block "default" // ParseCaseBlock
    // razor keywords
    keywords.["section"]    <- parse_section 
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
        | Success(result, _, _)   -> result 
        | Failure(errorMsg, _, _) -> failwith errorMsg

    let parse_from_stream (stream:Stream) (streamName:string) (enc:Encoding) = 
        match runParserOnStream grammar UState.Default streamName stream enc with 
        | Success(result, _, _)   -> result 
        | Failure(errorMsg, _, _) -> failwith errorMsg
    
    let parse_string (content:string) = 
        match runParserOnString grammar UState.Default "" content with 
        | Success(result, _, _)   -> result 
        | Failure(errorMsg, _, _) -> failwith errorMsg

