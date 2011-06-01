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
    open Castle.MonoRail.Mvc.ViewEngines


    type public FormHelper(ctx) = 
        inherit BaseHelper(ctx)

        member x.FormFor(url:TargetUrl, inner:Func<FormBuilder, IHtmlString>) = 
            // x.InternalFormFor (url.Generate null) "post" null
            let builder = new FormBuilder(x.Writer, "root")
            inner.Invoke(builder)


        (*
        member x.FormFor (url:TargetUrl) = 
            x.InternalFormFor (url.Generate null) "post" null

        member x.FormFor ((url:TargetUrl), urlparameters) = 
            x.InternalFormFor (url.Generate urlparameters) "post" null

        member x.FormFor ((url:TargetUrl), urlparameters, attributes) = 
            x.InternalFormFor (url.Generate urlparameters) "post" attributes
        *)

        member internal x.InternalFormFor (url:string) ``method`` (attributes:IDictionary<string,string>) = 
            let html = sprintf "<form action=\"%s\" method=\"%s\" %s>" url ``method`` (x.AttributesToString attributes)
            base.Writer.WriteLine html
            new FormBuilder(x.Writer, "root")


    and FormBuilder(writer, name) = 
        let _writer = writer

        member x.FieldsFor(inner:Func<FormBuilder, IHtmlString>) = 
            let builder = new FormBuilder(_writer, "sec")
            inner.Invoke(builder) |> ignore
            ()
        
        member x.Label() =
            ()

        member x.TextField() =
            ()

        member x.TextArea() =
            ()

        member x.FileField() =
            ()
        
        override x.ToString() = 
            name

        interface IDisposable with
            member x.Dispose() = 
                _writer.WriteLine "</form>"
                ()

