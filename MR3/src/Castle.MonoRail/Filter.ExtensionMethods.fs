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

namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility
    open Castle.MonoRail.Hosting.Mvc.Typed

    [<System.Runtime.CompilerServices.ExtensionAttribute>]
    module ExtensionMethods = 

        let internal get_list (route:Route) =
            if not (route.ExtraData.ContainsKey(Constants.MR_Filters_Key)) then
                route.ExtraData.[Constants.MR_Filters_Key] <- List<FilterDescriptor>()

            route.ExtraData.[Constants.MR_Filters_Key] :?> List<FilterDescriptor>

        //i'm sure that the naming can be improved here. applyfilter? withfilter?
        [<System.Runtime.CompilerServices.ExtensionAttribute>]
        let SetFilter<'filter>(route:Route) = 
            let descriptors = get_list route

            descriptors.Add(FilterDescriptor(typeof<'filter>))
            route

        // same here: onexception? onerrorexecute?
        [<System.Runtime.CompilerServices.ExtensionAttribute>]
        let SetExceptionFilter<'filter, 'excp when 'filter :> IExceptionFilter and 'excp :> Exception>(route:Route) = 
            let descriptors = get_list route

            descriptors.Add(ExceptionFilterDescriptor(typeof<'filter>, typeof<'excp>))
            route