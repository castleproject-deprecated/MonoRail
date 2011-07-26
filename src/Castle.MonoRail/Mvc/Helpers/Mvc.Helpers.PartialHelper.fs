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

namespace Castle.MonoRail.Helpers

    open System
    open System.IO
    open System.Collections.Generic
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines
    open Castle.MonoRail.Hosting.Mvc.Typed


    type PartialHelper<'a>(ctx, ctxmodel:'a, ctxbag:IDictionary<string,obj>) = 
        inherit BaseHelper(ctx)

        let _render (viewRequest:ViewRequest) partialName (renderer:ViewRendererService) ctx (output:TextWriter) bag (model) = 
            let partialReq = viewRequest.CreatePartialRequest partialName
            renderer.RenderPartial(partialReq, ctx, bag, model, output)

        member private x.ViewReq = x.ViewContext.ViewRequest

        member x.RenderAsResult(partialName) : IHtmlStringEx = 
            x.RenderAsResult (partialName, ctxmodel, ctxbag)

        member x.RenderAsResult(partialName:string, model:'a) : IHtmlStringEx = 
            x.RenderAsResult (partialName, model, ctxbag)

        member x.RenderAsResult(partialName:string, bag:IDictionary<string,obj>) : IHtmlStringEx = 
            x.RenderAsResult (partialName, ctxmodel, bag)

        member x.RenderAsResult(partialName:string, model:'a, bag:IDictionary<string,obj>) : IHtmlStringEx = 
            use writer = new StringWriter()
            _render (x.ViewReq) partialName (x.ServiceRegistry.ViewRendererService) (x.HttpContext) writer bag model 
            upcast HtmlResult(writer.GetStringBuilder().ToString())


        member x.Render(partialName) = 
            _render (x.ViewReq) partialName (x.ServiceRegistry.ViewRendererService) (x.HttpContext) (x.Writer) ctxbag ctxmodel 

        member x.Render(partialName:string, model:'a) = 
            _render (x.ViewReq) partialName (x.ServiceRegistry.ViewRendererService) (x.HttpContext) (x.Writer) ctxbag model 

        member x.Render(partialName:string, bag:IDictionary<string,obj>) = 
            _render (x.ViewReq) partialName (x.ServiceRegistry.ViewRendererService) (x.HttpContext) (x.Writer) bag ctxmodel 

        member x.Render(partialName:string, model:'a, bag:IDictionary<string,obj>) = 
            _render (x.ViewReq) partialName (x.ServiceRegistry.ViewRendererService) (x.HttpContext) (x.Writer) bag model 


        member x.Exists(partialName:string) =
            let partialReq = x.ViewReq.CreatePartialRequest partialName
            x.ServiceRegistry.ViewRendererService.HasPartialView (partialReq, x.HttpContext)

