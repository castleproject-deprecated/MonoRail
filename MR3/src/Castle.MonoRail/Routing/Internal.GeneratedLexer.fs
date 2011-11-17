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

namespace Castle.MonoRail.Routing

open System 

module SimpleTokensLex = 
 
    type token = 
        | ID     of string 
        | STRING of string 
        | DOT | OPEN | CLOSE | SLASH | COLON
        | EOF 
 
    let lexeme = Lexing.LexBuffer<_>.LexemeString 

    # 15 "lexical_def.fs"
    let trans : uint16[] array = 
        [| 
        (* State 0 *)
         [| 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 1us; 8us; 8us; 8us; 8us; 8us; 1us; 3us; 4us; 1us; 8us; 8us; 1us; 2us; 6us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 5us; 8us; 8us; 8us; 8us; 8us; 8us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 8us; 8us; 8us; 8us; 1us; 8us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 8us; 8us; 8us; 1us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 8us; 7us; |];
        (* State 1 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 65535us; 65535us; 12us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 2 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 65535us; 65535us; 12us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 3 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 65535us; 65535us; 12us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 4 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 65535us; 65535us; 12us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 5 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 9us; 9us; 9us; 65535us; 65535us; 9us; 9us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 10us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 65535us; 65535us; 65535us; 65535us; 9us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 65535us; 65535us; 65535us; 9us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 6 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 7 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 8 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 9 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 11us; 11us; 11us; 65535us; 65535us; 11us; 11us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 65535us; 11us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 10 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 9us; 9us; 9us; 65535us; 65535us; 9us; 9us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 10us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 65535us; 65535us; 65535us; 65535us; 9us; 65535us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 9us; 65535us; 65535us; 65535us; 9us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 11 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 11us; 11us; 11us; 65535us; 65535us; 11us; 11us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 65535us; 11us; 65535us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 11us; 65535us; 65535us; 65535us; 11us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        (* State 12 *)
         [| 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 65535us; 65535us; 12us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 65535us; 12us; 65535us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 12us; 65535us; 65535us; 65535us; 12us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; 65535us; |];
        |] 
    let actions : uint16[] = [|65535us; 0us; 0us; 0us; 0us; 7us; 5us; 6us; 7us; 1us; 65535us; 1us; 0us; |]
    let _fslex_tables = Microsoft.FSharp.Text.Lexing.UnicodeTables.Create(trans,actions)
    let rec _fslex_dummy () = _fslex_dummy() 
    (* Rule token *)
    and token  (lexbuf : Microsoft.FSharp.Text.Lexing.LexBuffer<_>) = _fslex_token  0 lexbuf
    (* Rule token *)
    and _fslex_token  _fslex_state lexbuf =
      match _fslex_tables.Interpret(_fslex_state,lexbuf) with
      | 0 -> ( 
    # 33 "lexical_def.fsl"
                                   STRING (lexeme lexbuf)  
    # 56 "lexical_def.fs"
              )
      | 1 -> ( 
    # 34 "lexical_def.fsl"
                                      ID (lexeme lexbuf)  
    # 61 "lexical_def.fs"
              )
      | 2 -> ( 
    # 35 "lexical_def.fsl"
                                      DOT 
    # 66 "lexical_def.fs"
              )
      | 3 -> ( 
    # 36 "lexical_def.fsl"
                                      OPEN 
    # 71 "lexical_def.fs"
              )
      | 4 -> ( 
    # 37 "lexical_def.fsl"
                                      CLOSE 
    # 76 "lexical_def.fs"
              )
      | 5 -> ( 
    # 38 "lexical_def.fsl"
                                      SLASH 
    # 81 "lexical_def.fs"
              )
      | 6 -> ( 
    # 39 "lexical_def.fsl"
                                      EOF 
    # 86 "lexical_def.fs"
              )
      | 7 -> ( 
    # 40 "lexical_def.fsl"
                                      failwithf "unrecognized input: '%s'" (lexeme lexbuf) 
    # 91 "lexical_def.fs"
              )
      | _ -> failwith "token"

    # 41 "lexical_def.fsl"

    # 3000000 "lexical_def.fs"
