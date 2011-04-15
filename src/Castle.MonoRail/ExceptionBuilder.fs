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

module ExceptionBuilder
    
    open System

    let private NotImplemented = 
        "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

    let internal RaiseNotImplemented() = 
        raise (NotImplementedException(NotImplemented))

    let internal RaiseArgumentNull name = 
        let msg = sprintf "The argument %s is required. It cannot be null or empty" name
        raise (ArgumentNullException(msg))
        
