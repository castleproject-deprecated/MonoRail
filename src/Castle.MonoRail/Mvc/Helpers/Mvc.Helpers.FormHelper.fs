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
    open System.ComponentModel.DataAnnotations
    open System.Text
    open System.Globalization
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.ViewEngines


    type public FormHelper(ctx, metadataProvider:ModelMetadataProvider) = 
        inherit BaseHelper(ctx)

        let _formTagHelper = FormTagHelper(ctx)

        static member InferPrefix (modelType:Type) = 
            modelType.Name.ToLowerInvariant()

        // takes model
        member x.For(model:'TModel, url:TargetUrl, inner:Func<GenFormBuilder<'TModel>, HtmlResult>) : IHtmlStringEx = 
            let prefix = FormHelper.InferPrefix typeof<'TModel>
            let writer = new StringWriter()
            let builder = x.InternalFormFor(prefix, model, (url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "\r\n</form>"
            upcast HtmlResult( writer.ToString() )

        // non generic version
        (*
        member x.For(url:TargetUrl, prefix, inner:Func<FormBuilder, HtmlResult>) : IHtmlStringEx = 
            let writer = new StringWriter()
            let builder : FormBuilder = x.InternalFormFor(prefix, (url.Generate null), "post", null, writer)
            inner.Invoke(builder).WriteTo(writer)
            writer.WriteLine "</form>"
            upcast HtmlResult( writer.ToString() )
        *)

        member internal x.InternalFormFor(prefix:string, model:'TModel, url:string, ``method``, html:IDictionary<string,string>, writer:TextWriter) = 
            _formTagHelper.FormTag(url, ``method``, prefix + "_form", html).WriteTo writer
            writer.WriteLine()
            let metadata = metadataProvider.Create(typeof<'TModel>)
            GenFormBuilder(prefix, writer, _formTagHelper, model, metadata, metadataProvider)

        (*
        member internal x.InternalFormFor(prefix:string, url:string, ``method``, html:IDictionary<string,string>, writer:TextWriter) = 
            _formTagHelper.FormTag(url, ``method``, prefix + "_form", html).WriteTo writer
            writer.WriteLine()
            FormBuilder(prefix, writer, _formTagHelper)
        *)


    and FormBuilder(prefix, writer, formTagHelper) = 

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
        

    and GenFormBuilder<'TModel>(prefix, writer, formTagHelper, model:'TModel, modelmetadata, metadataProvider) = 
        inherit FormBuilder(prefix, writer, formTagHelper)

        let mutable _formTemplate : Func<TemplateFormBuilder, HtmlResult> = null

        let getModelMeta (memberAccesses:PropertyInfo[]) = 
            let targetProp = Array.get memberAccesses (memberAccesses.Length - 1)
            if typeof<'TModel> = targetProp.ReflectedType then
                modelmetadata.GetPropertyMetadata targetProp
            else
                metadataProvider.Create(targetProp.ReflectedType).GetPropertyMetadata targetProp

        let buildNames (memberAccesses:PropertyInfo[]) = 
            let namebuf = StringBuilder(prefix)
            let idbuf = StringBuilder(prefix)
            for memberAccess in memberAccesses do
                let name = memberAccess.Name.ToLowerInvariant()
                namebuf.Append (sprintf "[%s]" name) |> ignore
                idbuf.Append (sprintf "_%s" name) |> ignore
            (namebuf.ToString(), idbuf.ToString())

        (* 
        member x.FieldsFor(inner:Func<FormBuilder<'a>, HtmlResult>) : IHtmlStringEx =
            // let writer = new StringWriter()
            // inner.Invoke(this).WriteTo(writer)
            // HtmlString( (writer.ToString()) ) :> IHtmlString
            upcast HtmlResult ""
        *)
        
        member x.FormTemplate (template:Func<TemplateFormBuilder, HtmlResult>) = 
            _formTemplate <- template

        member x.TemplateFor(propertyAccess:Expression<Func<'TModel, obj>>) : IHtmlStringEx =
            if _formTemplate == null then failwith "No template set. Call FormTemplate first"

            let memberAccesses = propinfo_from_exp propertyAccess
            let name, idVal = buildNames memberAccesses
            let modelMeta = getModelMeta memberAccesses
            
            let templateCtx = 
                TemplateFormBuilder( 
                    (fun _ -> x.InternalLabelFor(modelMeta, idVal)), 
                    (fun _ -> x.InternalEditorFor(name, idVal, modelMeta)) )
            upcast _formTemplate.Invoke(templateCtx)

        member this.LabelFor(propertyAccess:Expression<Func<'TModel, obj>>) : IHtmlStringEx =
            let memberAccesses = propinfo_from_exp propertyAccess
            let _, idVal = buildNames memberAccesses
            let modelMeta = getModelMeta memberAccesses
            this.InternalLabelFor(modelMeta, idVal)

        member x.DisplayForModel() : IHtmlStringEx =
            upcast HtmlResult ""

        member x.EditorForModel() : IHtmlStringEx =
            upcast HtmlResult ""

        member x.DisplayFor() : IHtmlStringEx =
            upcast HtmlResult ""

        member x.EditorFor(propertyAccess:Expression<Func<'TModel, obj>>) : IHtmlStringEx =
            let memberAccesses = propinfo_from_exp propertyAccess
            let propMetadata = getModelMeta memberAccesses
            let nameVal, idVal = buildNames memberAccesses
            x.InternalEditorFor (nameVal, idVal, propMetadata)



        member internal this.InternalLabelFor (modelMeta:ModelMetadata, id:string) : IHtmlStringEx =
            formTagHelper.LabelTag(modelMeta.DisplayName, id)

        member internal x.InternalEditorFor (nameVal, idVal, propMetadata:ModelMetadata) : IHtmlStringEx =
            let tuples = 
                seq {
                        if propMetadata.DefaultValue != null then 
                            yield ("placeholder", propMetadata.DefaultValue.ToString()) 
                    }
            let isRequired = propMetadata.Required != null
            let htmlAtts = Map(tuples)
            // TODO: only supports one level of depth
            let propVal = propMetadata.GetValue(model) // propertyAccess.Compile().Invoke(model)
            let valueStr = if propVal == null then null else propVal.ToString()

            match propMetadata.DataType with 
            | DataType.Text ->
                formTagHelper.TextFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = valueStr, required = isRequired, html = htmlAtts)
            | DataType.Date ->
                formTagHelper.DateYMDFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = (propVal :?> DateTime), required = isRequired, html = htmlAtts)
            | DataType.EmailAddress ->
                formTagHelper.EmailFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = valueStr, required = isRequired, html = htmlAtts)
            | DataType.Password ->
                formTagHelper.PasswordFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = valueStr, required = isRequired, html = htmlAtts)
            | DataType.PhoneNumber ->
                formTagHelper.PhoneFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = valueStr, required = isRequired, html = htmlAtts)
            | DataType.Url ->
                formTagHelper.UrlFieldTag(
                                name = nameVal, 
                                id = idVal, 
                                value = valueStr, required = isRequired, html = htmlAtts)
            | _ ->
                failwithf "DataType not support for FieldFor. DataType: %O" (propMetadata.DataType)



    and TemplateFormBuilder ( label:unit -> IHtmlStringEx, field:unit -> IHtmlStringEx ) = 
        class
            member x.Label() : IHtmlStringEx =
                label()
            
            member x.Field() : IHtmlStringEx =
                field()
        end
            







