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
            | MarkupWithinElement of string * ASTNode
            | Code of string
            | CodeBlock of ASTNode list
            | Lambda of string list * ASTNode
            | Invocation of string * ASTNode
            | IfElseBlock of ASTNode * ASTNode * ASTNode option
            | Param of ASTNode list
            | Comment 
            | Member of string
            | KeywordConditionalBlock of string * ASTNode * ASTNode
            | KeywordBlock of string * string * ASTNode
            | ImportNamespaceStmt of string
            | InheritStmt of string
            | HelperDecl of string * ASTNode * ASTNode
            | FunctionsBlock of string
            | None
