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
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines


    [<AbstractClass>]
    type public BaseHelper(context:ViewContext) = 
        let _ctx = context

        member x.Context = _ctx
        member x.Writer = _ctx.Writer
        member internal x.AttributesToString(attributes:IDictionary<string,string>) = 
            if (attributes == null) then
                ""
            else
                let buffer = StringBuilder()
                for pair in attributes do
                    buffer.Append(" ").Append(pair.Key).Append("=\"").Append(pair.Value).Append("\"") |> ignore
                buffer.ToString()
        member x.Encode str = _ctx.HttpContext.Server.HtmlEncode str




(*
type public HtmlHelper(ctx) = 
    inherit BaseHelper(ctx)

    member x.Label(id:string, text:string) =
        HtmlString("<label for='' >" + (text |> x.Encode) + "</label>")

    member x.TextInput(name:string) = 
        HtmlString("<input />")
*)








