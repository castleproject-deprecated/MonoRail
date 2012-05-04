//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Extension.OData

open System
open System.Collections
open System.Collections.Specialized
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open System.Linq
open System.Linq.Expressions
open System.Text
open System.Web
open Castle.MonoRail
open FParsec
open FParsec.Primitives
open FParsec.CharParsers


type BinaryOp = 
    | And   = 0
    | Or    = 1
    | Eq    = 2
    | Neq   = 3
    | Mul   = 4
    | Div   = 5
    | Mod   = 6
    | Add   = 7
    | Sub   = 8
    | LessT = 9
    | GreatT = 10
    | LessET = 11
    | GreatET= 12

type UnaryOp = 
    | Negate = 0
    | Not    = 1
    // | Cast   
    // | IsOf

type EdmPrimitives = 
    | Null = 0
    | Binary = 1
    | Boolean = 2
    | Byte = 3
    | DateTime = 4
    | Decimal = 5
    | Double = 6
    | Single = 7
    | Guid = 8
    | Int16 = 9
    | Int32 = 10
    | Int64 = 11
    | SByte = 12
    | SString = 13
    | Time = 14
    | String = 15
    | DateTimeOffset = 16
    

type Exp = 
    | Identifier of string
    | Element 
    | Literal of EdmPrimitives * string
    | MemberAccess of Exp * Exp  // string * (string list) option
    | Unary  of UnaryOp * Exp
    | Binary of Exp * BinaryOp * Exp
    // | MethodCall
    // | Paren
    // | Cast
    // | IsOf
    // | FuncCall
    with 
        member x.ToStringTree() = 
            let b = StringBuilder()
            let rec print (n) (level:int) = 
                b.AppendLine() |> ignore
                for x in 1..level do b.Append("  ") |> ignore
                match n with 
                | Identifier f          -> b.Append (sprintf "Id %s" f) |> ignore
                | Element               -> b.Append (sprintf "Element") |> ignore
                | Literal (t,v)         -> b.Append (sprintf "Literal %O [%s]" t v ) |> ignore
                | MemberAccess (ex,n)   -> 
                    b.Append (sprintf "MemberAccess " ) |> ignore
                    print ex (level + 1)
                    print n (level + 1)
                | Unary (op,ex)         -> 
                    b.Append (sprintf "Unary %O " op) |> ignore
                    print ex (level + 1)
                | Binary (ex1, op, ex2) -> 
                    b.Append (sprintf "Binary %O " op) |> ignore
                    print ex1 (level + 1)
                    print ex2 (level + 1)
                | _ -> failwithf "Unsupported node type? Need to update this match, dude"
            print x 1
            b.ToString()

// rename to queryparser instead?
module QueryExpressionParser =
    begin
        (*
                        OData/Operator Precedence
        Grouping                (x)                     parenExpression, boolParenExpression
        Primary                 x/m                     memberExpression
        Primary                 x(...)                  methodCallExpression, boolMethodCallExpression
        Unary                   -x                      negateExpression
        Unary                   not x                   notExpression
        Unary                   cast(T), cast(x, T)     castExpression
        Multiplicative          x mul y                 mulExpression
        Multiplicative          x div y                 divExpression
        Multiplicative          x mod y                 modExpression
        Additive                x add y                 addExpression
        Additive                x sub y                 subExpression
        Relational/ttesting     x lt y                  ltExpression
        Relational/ttesting     x gt y                  gtExpression
        Relational/ttesting     x le y                  leExpression
        Relational/ttesting     x ge y                  geExpression
        Relational/ttesting     isof(T), isof(x, T)     isofExpression
        Equality                x eq y                  eqExpression
        Equality                x ne y                  neExpression
        Conditional AND         x and y                 andExpression
        Conditional OR          x or y                  orExpression

        *)

        // Rewrites the tree to be rooted with MemberAccess(Element instead)
        // not ideal, but couldnt get fparsec to produce the desired tree 
        let rec rebuildMemberAccessTree exp = 
            match exp with 
            | MemberAccess (r, arg) -> MemberAccess(rebuildMemberAccessTree r, arg)
            | Identifier i          -> MemberAccess(Exp.Element, exp)
            | _                     -> failwithf "Not supposed to traverse any other node type, but got into %O" exp

        let ws      = spaces
        // let nospace = preturn ()
        let pc c            = ws >>. pchar c
        let pstr s          = ws >>. pstring s
        let lparen          = pstring "(" >>. ws
        let rparen          = pstring ")" >>. ws
        let opp             = new OperatorPrecedenceParser<_,_,_>()
        let ida             = identifier(IdentifierOptions())
        let entity          = ws >>. ida .>> manyChars (noneOf "/")

        // Address               MemberAccess(element, PriceAddress)
        // Address/Name          MemberAccess(MemberAccess(element, Address), Name)
        // Address/Name/Length   MemberAccess(MemberAccess(MemberAccess(element, Address), Name), Length)
        let combine         = stringReturn "/" (fun x y -> Exp.MemberAccess(x, y))
        let idAsExp         = ida .>> ws |>> (fun id -> Exp.Identifier(id))
        let memberAccessExp = chainl1 (idAsExp) (combine)
        
        let intLiteral      = many1Chars digit .>> ws |>> fun v -> Exp.Literal(EdmPrimitives.Int32, v)
        let decLiteral      = many1Chars digit .>> pchar '.' .>>. many1Chars digit .>> ws 
                                                                     |>> fun (v, d) -> Exp.Literal(EdmPrimitives.Single, (v + "." + d))
        let stringLiteral   = between (pc '\'') (pchar '\'') 
                                    (many1Chars (noneOf "'")) .>> ws |>> fun en -> Exp.Literal(EdmPrimitives.SString, en)
        let boolLiteral     = (pstr "true" <|> pstr "false")         |>> fun v  -> Exp.Literal(EdmPrimitives.Boolean, v)
        let literalExp      = attempt(decLiteral) <|> intLiteral <|> stringLiteral <|> boolLiteral

        let tryBetweenParens p = lparen >>? (p .>>? rparen)

        let exp             = opp.ExpressionParser
        let units           = (memberAccessExp |>> rebuildMemberAccessTree) <|> literalExp <|> tryBetweenParens exp 
      
        opp.TermParser <- units

        let term = exp .>> eof

        (* 
methodCallExpression = boolMethodExpression
                        / indexOfMethodCallExpression
                        / replaceMethodCallExpression
                        / toLowerMethodCallExpression
                        / toUpperMethodCallExpression
                        / trimMethodCallExpression
                        / substringMethodCallExpression
                        / concatMethodCallExpression
                        / lengthMethodCallExpression
                        / yearMethodCallExpression
                        / monthMethodCallExpression
                        / dayMethodCallExpression
                        / hourMethodCallExpression
                        / minuteMethodCallExpression
                        / secondMethodCallExpression
                        / roundMethodCallExpression
                        / floorMethodCallExpression
                        / ceilingMethodCallExpression

boolMethodExpression = endsWithMethodCallExpression
                        / startsWithMethodCallExpression
                        / substringOfMethodCallExpression        
        *)
        
        opp.AddOperator(InfixOperator("and",  ws, 1 , Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.And, y)))
        opp.AddOperator(InfixOperator("or",   ws, 2 , Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.Or, y)))

        opp.AddOperator(InfixOperator("eq",   ws, 3 , Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.Eq, y)))
        opp.AddOperator(InfixOperator("ne",   ws, 4 , Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.Neq, y)))
        
        opp.AddOperator(InfixOperator("lt" ,  ws, 5, Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.LessT, y)))
        opp.AddOperator(InfixOperator("gt" ,  ws, 6, Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.GreatT, y)))
        opp.AddOperator(InfixOperator("le" ,  ws, 7, Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.LessET, y)))
        opp.AddOperator(InfixOperator("ge" ,  ws, 8, Associativity.Right, fun x y -> Exp.Binary(x, BinaryOp.GreatET, y)))
        // opp.AddOperator(PrefixOperator("isof",nospace, 70, false, fun x -> Exp.Unary(UnaryOp.IsOf, x))

        opp.AddOperator(InfixOperator("add",  ws, 9, Associativity.Left, fun x y -> Exp.Binary(x, BinaryOp.Add, y)))
        opp.AddOperator(InfixOperator("sub",  ws, 10, Associativity.Left, fun x y -> Exp.Binary(x, BinaryOp.Sub, y)))

        opp.AddOperator(InfixOperator("mul",  ws, 11, Associativity.Left, fun x y -> Exp.Binary(x, BinaryOp.Mul, y)))
        opp.AddOperator(InfixOperator("div",  ws, 12, Associativity.Left, fun x y -> Exp.Binary(x, BinaryOp.Div, y)))
        opp.AddOperator(InfixOperator("mod",  ws, 13, Associativity.Left, fun x y -> Exp.Binary(x, BinaryOp.Mod, y)))
        
        opp.AddOperator(PrefixOperator("-",   ws, 14, false, fun x -> Exp.Unary(UnaryOp.Negate, x)))
        opp.AddOperator(PrefixOperator("not", ws, 15, false, fun x -> Exp.Unary(UnaryOp.Not, x)))
        // opp.AddOperator(PrefixOperator("cast", nospace, 100, false, fun x -> Exp.Unary(UnaryOp.Cast, x))

        let parse (original:string) = 
            let r = 
                match run term original with 
                | Success(result, _, _) -> result
                | Failure(errorMsg, _, _) -> (raise(ArgumentException(errorMsg)))
            r

    end

(* 
commonExpression = [WSP] (boolCommonExpression / 
                    methodCallExpression /
                    parenExpression / 
                    literalExpression / 
                    addExpression /
                    subExpression / 
                    mulExpression / 
                    divExpression /
                    modExpression / 
                    negateExpression / 
                    memberExpression / 
                    firstMemberExpression / 
                    castExpression) [WSP]

boolCommonExpression = [WSP] 
                       (boolLiteralExpression / 
                        andExpression /
                        orExpression /
                        boolPrimitiveMemberExpression / 
                        eqExpression / 
                        neExpression /
                        ltExpression / 
                        leExpression / 
                        gtExpression /
                        geExpression / 
                        notExpression / 
                        isofExpression/
                        boolCastExpression / 
                        boolMethodCallExpression /
                        firstBoolPrimitiveMemberExpression / 
                        boolParenExpression) [WSP]
                    
parenExpression     = "(" [WSP] commonExpression [WSP] ")"
boolParenExpression = "(" [WSP] boolCommonExpression [WSP] ")"

negateExpression    = "-" [WSP] commonExpression
notExpression       = "not" WSP commonExpression

isofExpression      = "isof" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP]stringLiteral [WSP] ")"
castExpression      = "cast" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP]stringLiteral [WSP] ")"
boolCastExpression  = "cast" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP] "Edm.Boolean" [WSP] ")"

firstMemberExpression = [WSP] 
                        entityNavProperty / ; section 2.2.3.1
                        entityComplexProperty / ; section 2.2.3.1
                        entitySimpleProperty ; section 2.2.3.1
                        
firstBoolPrimitiveMemberExpression = entityProperty ; section 2.2.3.1

memberExpression    = commonExpression [WSP] "/" [WSP]
                      entityNavProperty / ; section 2.2.3.1
                      entityComplexProperty / ; section 2.2.3.1
                      entitySimpleProperty ; section 2.2.3.1

boolPrimitiveMemberExpression = commonExpression [WSP] "/" [WSP]
                            entityProperty
                            ; section 2.2.3.1

literalExpression = stringLiteral ; section 2.2.2
                    / dateTimeLiteral ; section 2.2.2
                    / decimalLiteral ; section 2.2.2
                    / guidUriLiteral ; section 2.2.2
                    / singleLiteral ; section 2.2.2
                    / doubleLiteral ; section 2.2.2
                    / int16Literal ; section 2.2.2
                    / int32Literal ; section 2.2.2
                    / int64Literal ; section 2.2.2
                    / binaryLiteral ; section 2.2.2
                    / nullLiteral ; section 2.2.2
                    / byteLiteral ; section 2.2.2

boolLiteralExpression = boolLiteral ; section 2.2.2

methodCallExpression = boolMethodExpression
                        / indexOfMethodCallExpression
                        / replaceMethodCallExpression
                        / toLowerMethodCallExpression
                        / toUpperMethodCallExpression
                        / trimMethodCallExpression
                        / substringMethodCallExpression
                        / concatMethodCallExpression
                        / lengthMethodCallExpression
                        / yearMethodCallExpression
                        / monthMethodCallExpression
                        / dayMethodCallExpression
                        / hourMethodCallExpression
                        / minuteMethodCallExpression
                        / secondMethodCallExpression
                        / roundMethodCallExpression
                        / floorMethodCallExpression
                        / ceilingMethodCallExpression

boolMethodExpression = endsWithMethodCallExpression
                        / startsWithMethodCallExpression
                        / substringOfMethodCallExpression
                        
endsWithMethodCallExpression = "endswith" [WSP]
                                "(" [WSP] commonexpression [WSP]
                                "," [WSP] commonexpression [WSP] ")"

indexOfMethodCallExpression = "indexof" [WSP]
                            "(" [WSP] commonexpression [WSP]
                            "," [WSP] commonexpression [WSP] ")"

replaceMethodCallExpression = "replace" [WSP]
                            "(" [WSP] commonexpression [WSP]
                            "," [WSP] commonexpression [WSP]
                            "," [WSP] commonexpression [WSP] ")"

startsWithMethodCallExpression = "startswith" [WSP]
                                "(" [WSP] commonexpression [WSP]
                                "," [WSP] commonexpression [WSP] ")"

toLowerMethodCallExpression = "tolower" [WSP]
                                "(" [WSP] commonexpression [WSP] ")"

toUpperMethodCallExpression = "toupper" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

trimMethodCallExpression = "trim" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

substringMethodCallExpression = "substring" [WSP]
                                "(" [WSP] commonexpression [WSP]
                                [ "," [WSP] commonexpression [WSP] ] ")"					
                    
substringOfMethodCallExpression = "substringof" [WSP]
                                "(" [WSP] commonexpression [WSP]
                                [ "," [WSP] commonexpression [WSP] ] ")"

concatMethodCallExpression = "concat" [WSP]
                            "(" [WSP] commonexpression [WSP]
                            [ "," [WSP] commonexpression [WSP] ] ")"

lengthMethodCallExpression = "length" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

yearMethodCallExpression = "year" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

monthMethodCallExpression = "month" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

dayMethodCallExpression = "day" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

hourMethodCallExpression = "hour" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

minuteMethodCallExpression = "minute" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

secondMethodCallExpression = "second" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

roundMethodCallExpression = "round" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

floorMethodCallExpression = "floor" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"

ceilingMethodCallExpression = "ceiling" [WSP]
                            "(" [WSP] commonexpression [WSP] ")"					



SQUOTE = '          ; ' (single quote)
nonZeroDigit = 1-9 ; all digits except zero
doubleZeroToSixty =   "0" DIGIT
                    / "1" DIGIT
                    / "2" DIGIT
                    / "3" DIGIT
                    / "4" DIGIT
                    / "5" DIGIT
                    / "6" DIGIT
nan = "Nan"
negativeInfinity = "-INF"
postiveInfinity = "INF"
sign = "-" / "+"
DIGIT = ; see [RFC5234] Appendix A
UTF8-char = ; see [RFC3629]
*)