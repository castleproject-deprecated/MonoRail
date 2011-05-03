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
open System.Runtime.Serialization

[<Serializable>]
type RouteException = 
    inherit Exception
    new (msg) = { inherit Exception(msg) }
    new (info:SerializationInfo, context:StreamingContext) = 
        { 
            inherit Exception(info, context)
        }

module ExceptionBuilder = 
    
    open System

    let internal ArgumentNull name = 
        sprintf "The argument %s is required. It cannot be null or empty" name

    let internal UnexpectedNull name = 
        sprintf "Looks like something went very wrong. We expected to have a value for '%s' but it's actually null" name

    let internal NotImplemented =
        "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

    let internal UnexpectedToken name = 
        sprintf "Error parsing the route matching expression. We hit an unexpected token '%s' which indicates the expression is probably wrong. If it's not, its a bug" name

    let internal UnexpectedEndTokenStream = 
        "Unexpected end of token stream - I don't think the route path is well-formed"

    let internal RaiseNotImplemented() = 
        raise (NotImplementedException(NotImplemented))

    let internal RaiseRouteException msg = 
        raise (RouteException(msg))
