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
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines


    type public FormHelper(ctx) = 
        inherit BaseHelper(ctx)

        member x.FormFor(url:TargetUrl, inner:Func<FormBuilder, IHtmlString>) : IHtmlString = 
            let writer = new StringWriter()
            let builder = x.InternalFormFor (url.Generate null) "post" null writer
            writer.WriteLine (inner.Invoke(builder).ToString())
            writer.WriteLine "</form>"
            HtmlString( writer.ToString() ) :> IHtmlString

        (*
        member x.FormFor (url:TargetUrl) = 
            x.InternalFormFor (url.Generate null) "post" null

        member x.FormFor ((url:TargetUrl), urlparameters) = 
            x.InternalFormFor (url.Generate urlparameters) "post" null

        member x.FormFor ((url:TargetUrl), urlparameters, attributes) = 
            x.InternalFormFor (url.Generate urlparameters) "post" attributes
        *)

        member internal x.InternalFormFor (url:string) ``method`` (attributes:IDictionary<string,string>) (writer:TextWriter) = 
            let html = sprintf "<form action=\"%s\" method=\"%s\" %s>" url ``method`` (x.AttributesToString attributes)
            writer.WriteLine html
            FormBuilder(writer, "root")


    and FormBuilder(writer, name) = 
        let _writer = writer

        member x.FieldsFor(inner:Func<FormBuilder, IHtmlString>) = 
            let writer = new StringWriter()
            writer.WriteLine (inner.Invoke(x).ToString())
            HtmlString( (writer.ToString()) ) :> IHtmlString
        
        member x.Label(name:string) : IHtmlString =
            HtmlString(name) :> IHtmlString

        member x.TextField(name:string) : IHtmlString =
            HtmlString(name) :> IHtmlString

        member x.TextArea() =
            ()

        member x.FileField() =
            ()
        
        override x.ToString() = 
            name


