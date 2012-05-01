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
open System.Web
open Castle.MonoRail


// $filter=/Products?$filter=Address/City ne 'London' 

type FilterAst = 
    | Exp 
    | Op 
    | Unary 
    | BinaryExp 

module FilterParser =
    begin

    end

(* 

commonExpression = [WSP] (boolCommonExpression / methodCallExpression /
					parenExpression / literalExpression / addExpression /
					subExpression / mulExpression / divExpression /
					modExpression / negateExpression / memberExpression
					/ firstMemberExpression / castExpression) [WSP]

boolCommonExpression = [WSP] (boolLiteralExpression / andExpression /
					orExpression /
					boolPrimitiveMemberExpression / eqExpression / neExpression /
					ltExpression / leExpression / gtExpression /
					geExpression / notExpression / isofExpression/
					boolCastExpression / boolMethodCallExpression /
					firstBoolPrimitiveMemberExpression / boolParenExpression) [WSP]
					
parenExpression = "(" [WSP] commonExpression [WSP] ")"
boolParenExpression = "(" [WSP] boolCommonExpression [WSP] ")"
andExpression = boolCommonExpression WSP "and" WSP boolCommonExpression
orExpression  = boolCommonExpression WSP "or" WSP boolCommonExpression
eqExpression  = commonExpression WSP "eq" WSP commonExpression
neExpression  = commonExpression WSP "ne" WSP commonExpression
ltExpression  = commonExpression WSP "lt" WSP commonExpression
leExpression  = commonExpression WSP "le" WSP commonExpression
gtExpression  = commonExpression WSP "gt" WSP commonExpression
geExpression  = commonExpression WSP "ge" WSP commonExpression
addExpression = commonExpression WSP "add" WSP commonExpression
subExpression = commonExpression WSP "sub" WSP commonExpression
mulExpression = commonExpression WSP "mul" WSP commonExpression
divExpression = commonExpression WSP "div" WSP commonExpression
modExpression = commonExpression WSP "mod" WSP commonExpression

negateExpression = "-" [WSP] commonExpression
notExpression = "not" WSP commonExpression

isofExpression = "isof" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP]stringLiteral [WSP] ")"
castExpression = "cast" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP]stringLiteral [WSP] ")"
boolCastExpression = "cast" [WSP] "("[[WSP] commonExpression [WSP] ","][WSP] "Edm.Boolean" [WSP] ")"
firstMemberExpression = [WSP] entityNavProperty / ; section 2.2.3.1
						entityComplexProperty / ; section 2.2.3.1
						entitySimpleProperty ; section 2.2.3.1
						
firstBoolPrimitiveMemberExpression = entityProperty ; section 2.2.3.1

memberExpression = commonExpression [WSP] "/" [WSP]
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

*)