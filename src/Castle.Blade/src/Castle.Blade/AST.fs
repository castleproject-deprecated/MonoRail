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
            | KeywordBlock of string * ASTNode
            | None
