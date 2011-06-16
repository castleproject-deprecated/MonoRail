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
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines
    open Newtonsoft.Json
    open Castle.MonoRail.Hosting.Mvc.Typed

    [<AbstractClass>]
    type public BaseHelper(context:ViewContext) = 

        member x.Context = context
        member x.Writer = context.Writer
        member x.Encode str = context.HttpContext.Server.HtmlEncode str
        member internal x.AttributesToString(attributes:IDictionary<string,string>) = 
            if (attributes == null) then
                ""
            else
                let buffer = StringBuilder()
                for pair in attributes do
                    buffer.Append(" ").Append(pair.Key).Append("=\"").Append(pair.Value).Append("\"") |> ignore
                buffer.ToString()


    type public JsonHelper(context:ViewContext) = 
        
        member x.ToJson(graph:obj) = 
            let settings = JsonSerializerSettings() 
            let serializer = JsonSerializer.Create(settings)
            let writer = new StringWriter()
            serializer.Serialize( writer, graph )
            HtmlString( writer.GetStringBuilder().ToString() )


    type public PartialHelper<'a>(context:ViewContext, reg:IServiceRegistry, model:'a, bag:IDictionary<string,obj>) = 

        member x.Render(partialName) = 
            let partialReq = context.ViewRequest.CreatePartialRequest partialName
            reg.ViewRendererService.RenderPartial(partialReq, context.HttpContext, bag, model, context.Writer)

        member x.Render(partialName:string, model:'a) = 
            let partialReq = context.ViewRequest.CreatePartialRequest partialName
            reg.ViewRendererService.RenderPartial(partialReq, context.HttpContext, bag, model, context.Writer)

        member x.Render(partialName:string, bag:IDictionary<string,obj>) = 
            let partialReq = context.ViewRequest.CreatePartialRequest partialName
            reg.ViewRendererService.RenderPartial(partialReq, context.HttpContext, bag, model, context.Writer)

        member x.Render(partialName:string, model:'a, bag:IDictionary<string,obj>) = 
            let partialReq = context.ViewRequest.CreatePartialRequest partialName
            reg.ViewRendererService.RenderPartial(partialReq, context.HttpContext, bag, model, context.Writer)


    type public ViewComponentHelper(context:ViewContext, reg:IServiceRegistry) =

        member this.Render<'tvc when 'tvc :> IViewComponent>(configurer:Action<'tvc>) =
            reg.ViewComponentExecutor.Execute(typeof<'tvc>.Name, context.HttpContext, configurer)



