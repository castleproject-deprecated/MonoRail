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

namespace Castle.MonoRail.OData.Internal

    open System
    open System.IO
    open System.Linq
    open System.Collections
    open System.Collections.Generic
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Castle.MonoRail


    type MetadataProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response:ODataResponseMessage) = 
        inherit ODataSegmentProcessor(edmModel, odataModel, callbacks, parameters, serializer, request, response)

        override x.Process (op, segment, previous, hasMoreSegments, shouldContinue) = 
            match op with 
            | RequestOperation.Get ->
                serializer.SerializeMetadata(request, response)

            | _ -> failwithf "Unsupported operation %O at this level" op

            emptyResponse


