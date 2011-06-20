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

        // takes model
        member x.FormFor<'TModel when 'TModel:null>(model:'TModel, url:TargetUrl, inner:Func<FormBuilder<'TModel>, HtmlResult>) : IHtmlString = 
            let writer = new StringWriter()
            let builder : FormBuilder<'TModel> = x.InternalFormFor<'TModel>(model, (url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "</form>"
            upcast HtmlString( writer.ToString() )

        // non generic version
        member x.FormFor(url:TargetUrl, inner:Func<FormBuilder, HtmlResult>) : IHtmlString = 
            let writer = new StringWriter()
            let builder : FormBuilder = x.InternalFormFor((url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "</form>"
            upcast HtmlString( writer.ToString() )

        member internal x.InternalFormFor<'TModel when 'TModel:null> (model:'TModel, url:string, ``method``, attributes:IDictionary<string,string>, writer:TextWriter) = 
            let html = sprintf "<form action=\"%s\" method=\"%s\" %s>" url ``method`` (x.AttributesToString attributes)
            writer.WriteLine html
            FormBuilder<'TModel>(model, writer, "root")

        member internal x.InternalFormFor(url:string, ``method``, attributes:IDictionary<string,string>, writer:TextWriter) = 
            let html = sprintf "<form action=\"%s\" method=\"%s\" %s>" url ``method`` (x.AttributesToString attributes)
            writer.WriteLine html
            FormBuilder(writer, "root")


    and FormBuilder(writer, name) = 

        member this.Label(name:string) : IHtmlString =
            HtmlString(name) :> IHtmlString

        member this.TextField(name:string) : IHtmlString =
            // should we access the metadata here? I vote for no
            HtmlString(name) :> IHtmlString

        member this.TextArea() =
            ()

        member this.FileField() =
            ()
        
        override this.ToString() = 
            name

    and FormBuilder<'a when 'a:null>(model:'a, writer, name) = 
        inherit FormBuilder(writer, name)
        let _writer = writer

        member this.FieldsFor(inner:Func<FormBuilder<'a>, HtmlResult>) = 
            let writer = new StringWriter()
            inner.Invoke(this).WriteTo(writer)
            HtmlString( (writer.ToString()) ) :> IHtmlString
        

        member this.TextField(propertyAccess:Expression<Func<'a, obj>>) : IHtmlString =
            // access metadata
            HtmlString(name) :> IHtmlString

