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
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines


    [<Interface>]
    type IHtmlStringEx = 
        inherit IHtmlString
        abstract member WriteTo : writer:TextWriter -> unit


    type public HtmlResult (ac:Action<TextWriter>) = 
        
        new(content:string) = 
            HtmlResult((fun (w:TextWriter) -> w.Write content))

        override x.ToString() = 
            use writer = new StringWriter() 
            ac.Invoke(writer)
            writer.ToString()

        member x.WriteTo(writer:TextWriter) =
            ac.Invoke(writer)

        interface IHtmlStringEx with 
            member x.WriteTo(writer:TextWriter) =
                ac.Invoke(writer)

            member x.ToHtmlString() = x.ToString()


    type public HelperContext(context:ViewContext, metadataProvider:ModelMetadataProvider, registry:IServiceRegistry) = 
        class
            member x.ViewContext = context
            member x.ModelMetadataProvider = metadataProvider
            member x.ServiceRegistry = registry
            member x.Writer = context.Writer
            member x.HttpContext = context.HttpContext
        end


    [<AbstractClass>]
    type public BaseHelper(context:HelperContext) = 

        // instead of internal we should be using protected!
        member internal x.Context = context
        member internal x.ViewContext = context.ViewContext
        member internal x.ModelMetadataProvider = context.ModelMetadataProvider
        member internal x.ServiceRegistry = context.ServiceRegistry
        member internal x.Writer = context.Writer
        member internal x.HttpContext = context.HttpContext

        member internal x.HtmlEncode str = 
            context.HttpContext.Server.HtmlEncode str
        
        member internal x.UrlEncode str = 
            context.HttpContext.Server.UrlEncode str
        
        member internal x.AttributesToString(attributes:IDictionary<string,string>) = 
            if (attributes == null) then
                ""
            else
                let buffer = StringBuilder()
                for pair in attributes do
                    buffer.Append(" ").Append(pair.Key).Append("=\"").Append(pair.Value).Append("\"") |> ignore
                buffer.ToString()

        member internal x.Merge (html:IDictionary<string,string>) (kv:(string * string) seq) : IDictionary<string,string> = 
            let dict = 
                if html != null then Dictionary<string,string>(html) else Dictionary<string,string>()
            for k in kv do
                dict.[fst k] <- snd k
            upcast dict

        member internal x.ToId (name:string) =
            // inneficient!
            name.Replace("[", "_").Replace("]", "").Replace(".", "_")





