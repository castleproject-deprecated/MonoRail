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

module AST = 

    open FParsec

    type ASTNode = 
            | Markup of Position * string
            | MarkupBlock of ASTNode list
            | MarkupWithinElement of ASTNode * ASTNode 
            | Code of Position * string
            | CodeBlock of ASTNode list
            | Lambda of string list * ASTNode
            | Invocation of Position * string * ASTNode option
            | IfElseBlock of Position * ASTNode * ASTNode * ASTNode option
            | Param of ASTNode list
            | Bracket of string * ASTNode option
            | KeywordConditionalBlock of Position * string * ASTNode * ASTNode
            | KeywordBlock of Position * string * string * ASTNode
            | ImportNamespaceDirective of Position * string
            | FunctionsBlock of Position * string
            | InheritDirective of Position * string
            | HelperDecl of Position * string * ASTNode * ASTNode
            | TryStmt of Position * ASTNode * ASTNode list option * ASTNode option 
            | DoWhileStmt of Position * ASTNode * ASTNode
            | ModelDirective of Position * string
            | Comment
        with 
            override x.ToString() = 
                match x with 
                | Markup (_, s) -> sprintf "Markup %s" s
                | MarkupBlock s -> sprintf "MarkupBlock %A" s
                | MarkupWithinElement (node1,node2) -> sprintf "MarkupWithinElement %O %O" node1 node2
                | Code (_, s) -> sprintf "Code %s" s
                | CodeBlock s -> sprintf "CodeBlock %A" s
                | Invocation (pos, left, opt) -> sprintf "Invocation %s [%O]" left (if opt.IsSome then opt.Value.ToString() else "")
                | Param s -> sprintf "Param %A" s
                | _ -> x.GetType().FullName
            
            member x.ToList() : ASTNode list = 
                match x with 
                | MarkupBlock s -> s
                | CodeBlock s -> s
                | _ -> failwithf "Node is not a MarkupBlock or CodeBlock, so ToList failed. %O" x
            
            member x.Content() = 
                match x with 
                | Markup (_, s) -> s
                | _ -> x.GetType().Name