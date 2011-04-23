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

namespace Castle.MonoRail

    open System
    open System.Runtime.Serialization

    [<Serializable>]
    type MonoRailException = 
        inherit Exception
        new (msg) = { inherit Exception(msg) }
        new (info:SerializationInfo, context:StreamingContext) = 
            { 
                inherit Exception(info, context)
            }

// namespace Castle.MonoRail.Internal

    module ExceptionBuilder = 
    
        open System

        let private NotImplemented = 
            "You tried to use a functionality which has not been implemented. Looks like a great opportunity to contribute!"

        let internal RaiseNotImplemented() = 
            raise (NotImplementedException(NotImplemented))

        let internal RaiseArgumentNull name = 
            let msg = sprintf "The argument %s is required. It cannot be null or empty" name
            raise (ArgumentNullException(msg))
        
        let internal RaiseControllerProviderNotFound() = 
            // let msg = sprintf "No controller provider found" name
            let msg = "No controller provider found"
            raise (Exception(msg))

        let internal RaiseControllerExecutorProviderNotFound() = 
            // let msg = sprintf "No controller provider found" name
            let msg = "No controller executor provider found"
            raise (Exception(msg))    
    
        let internal RaiseMRException msg = 
            raise (MonoRailException(msg))

        let internal CandidatesNotFoundMsg name = 
            sprintf "No actions found matching requested action named %s" name

        let internal EmptyActionProcessors = 
            "No action processors found"