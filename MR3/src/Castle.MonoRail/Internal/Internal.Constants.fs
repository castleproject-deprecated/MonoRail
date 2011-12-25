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

module Constants

    let internal MR_Routing_Key = "mr_route_data"

    let internal MR_Filters_Key = "mr_filters_data"

    [<Literal>]
    let internal ActionProcessor_BeforeActionFilterProcessor        = 10000
    [<Literal>]
    let internal ActionProcessor_ActionParameterBinder              = 80000
    [<Literal>]
    let internal ActionProcessor_ActionExecutorProcessor            = 100000
//    [<Literal>]
//    let internal ActionProcessor_InvocationErrorProcessorProcessor  = 110000
    [<Literal>]
    let internal ActionProcessor_AfterActionFilterProcessor         = 120000 
    [<Literal>]
    let internal ActionProcessor_ActionResultExecutorProcessor      = 1000000
    [<Literal>]
    let internal ActionProcessor_ExecutionFilterProcessor           = 10000000