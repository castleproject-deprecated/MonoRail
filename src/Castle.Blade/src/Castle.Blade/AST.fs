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

    type ASTNode = 
            | Markup of string
            | MarkupBlock of ASTNode list
            | MarkupWithinElement of ASTNode * ASTNode 
            | Code of string
            | CodeBlock of ASTNode list
            | Lambda of string list * ASTNode
            | Invocation of string * ASTNode option
            | IfElseBlock of ASTNode * ASTNode * ASTNode option
            | Param of ASTNode list
            | KeywordConditionalBlock of string * ASTNode * ASTNode
            | KeywordBlock of string * string * ASTNode
            | ImportNamespaceDirective of string
            | FunctionsBlock of string
            | InheritDirective of string
            | HelperDecl of string * ASTNode * ASTNode
            | TryStmt of ASTNode * ASTNode list option * ASTNode option 
            | DoWhileStmt of ASTNode * ASTNode
            | ModelDirective of string
            | Comment
            | None
        with 
            override x.ToString() = 
                match x with 
                | Markup s -> sprintf "Markup %s" s
                | MarkupBlock s -> sprintf "MarkupBlock %A" s
                | MarkupWithinElement (node1,node2) -> sprintf "MarkupWithinElement %O %O" node1 node2
                | Code s -> sprintf "Code %s" s
                | CodeBlock s -> sprintf "CodeBlock %A" s
                | Invocation (left, opt) -> sprintf "Invocation %s [%O]" left (if opt.IsSome then opt.Value else None)
                | Param s -> sprintf "Param %A" s
                | _ -> x.GetType().FullName

                