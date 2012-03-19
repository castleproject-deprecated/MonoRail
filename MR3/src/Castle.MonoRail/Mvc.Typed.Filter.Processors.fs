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

namespace Castle.MonoRail.Hosting.Mvc.Typed
    
    open System
    open System.Collections.Generic
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility

    // Processors

    [<Export(typeof<ActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_AuthorizationFilter)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type AuthorizationFilterProcessor 
        [<ImportingConstructor>]
        ([<ImportMany>] providers:FilterProvider seq) =
        inherit ActionProcessor()

        override x.Process(context) = 
            
            let filters : IAuthorizationFilter seq = 
                providers 
                |> Seq.collect (fun p -> p.Provide(null, context) )
                
            if not <| Seq.isEmpty filters then 
                let filterCtx = PreActionFilterExecutionContext(context)
                for f in filters do 
                    f.AuthorizeRequest(filterCtx)
                    // if filterCtx.ActionResult <> null then
                        
                
                base.ProcessNext(context)
            else
                base.ProcessNext(context)



    (*

    [<Export(typeof<ActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_AfterActionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type AfterActionFilterProcessor() =
        inherit BaseFilterProcessor<IAfterActionFilter>((fun filter ctx -> filter.Execute(ctx)))


    [<Export(typeof<ActionProcessor>)>]
    [<ExportMetadata("Order", Constants.ActionProcessor_ExecutionFilterProcessor)>]
    [<PartMetadata("Scope", ComponentScope.Request)>]
    type ExceptionFilterProcessor() =
        inherit BaseFilterProcessor<IExceptionFilter>((fun filter ctx -> filter.Execute(ctx)))
    
    *)

