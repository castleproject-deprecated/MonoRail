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

        let _formTagHelper = FormTagHelper(ctx)

        static member InferPrefix (modelType:Type) = 
            modelType.Name.ToLowerInvariant()

        // takes model
        member x.FormFor<'TModel when 'TModel:null>(model:'TModel, url:TargetUrl, inner:Func<FormBuilder<'TModel>, HtmlResult>) : IHtmlStringEx = 
            let prefix = FormHelper.InferPrefix typeof<'TModel>
            let writer = new StringWriter()
            let builder : FormBuilder<'TModel> = x.InternalFormFor<'TModel>(prefix, model, (url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "</form>"
            upcast HtmlResult( writer.ToString() )

        // non generic version
        member x.FormFor(url:TargetUrl, inner:Func<FormBuilder, HtmlResult>) : IHtmlStringEx = 
            let prefix = FormHelper.InferPrefix typeof<'TModel>
            let writer = new StringWriter()
            let builder : FormBuilder = x.InternalFormFor(prefix, (url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "</form>"
            upcast HtmlResult( writer.ToString() )

        member internal x.InternalFormFor<'TModel when 'TModel:null>(prefix:string, model:'TModel, url:string, ``method``, html:IDictionary<string,string>, writer:TextWriter) = 
            _formTagHelper.FormTag(url, ``method``, prefix, html).WriteTo writer
            FormBuilder<'TModel>(model, prefix, writer, "root", _formTagHelper)

        member internal x.InternalFormFor(prefix:string, url:string, ``method``, html:IDictionary<string,string>, writer:TextWriter) = 
            _formTagHelper.FormTag(url, ``method``, prefix, html).WriteTo writer
            FormBuilder(prefix, writer, "root", _formTagHelper)


    and FormBuilder(prefix, writer, name, formTagHelper) = 

        member this.Label(name:string) : IHtmlStringEx =
            failwithf "not implemented"
            upcast HtmlResult ""

        member this.TextField(name:string) : IHtmlStringEx =
            // should we access the metadata here? I vote for no
            failwithf "not implemented"
            upcast HtmlResult ""

        member this.TextArea() : IHtmlStringEx =
            failwithf "not implemented"
            upcast HtmlResult ""

        member this.FileField() : IHtmlStringEx =
            failwithf "not implemented"
            upcast HtmlResult ""
        

    and FormBuilder<'a when 'a:null>(model:'a, prefix, writer, name, formTagHelper) = 
        inherit FormBuilder(prefix, writer, name, formTagHelper)

        (* 
        member x.FieldsFor(inner:Func<FormBuilder<'a>, HtmlResult>) : IHtmlStringEx =
            // let writer = new StringWriter()
            // inner.Invoke(this).WriteTo(writer)
            // HtmlString( (writer.ToString()) ) :> IHtmlString
            upcast HtmlResult ""
        *)
        
        member x.FieldFor(propertyAccess:Expression<Func<'a, obj>>) : IHtmlStringEx =
            

            // access metadata
            failwithf "not implemented"
            upcast HtmlResult ""


