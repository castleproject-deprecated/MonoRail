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

namespace Castle.MonoRail

    open System
    open System.Reflection
    open System.Collections.Generic
    open System.ComponentModel
    open System.ComponentModel.DataAnnotations
    open System.Web


    type ModelMetadata(valueAccessor:Func<obj>, properties:#IDictionary<PropertyInfo, ModelMetadata>) = 
        class
            let mutable _dataType : DataType = DataType.Text
            let mutable _displayFormat : DisplayFormatAttribute = null
            let mutable _display : DisplayAttribute = null
            let mutable _editable : EditableAttribute = null
            let mutable _UIHint  : UIHintAttribute = null
            let mutable _required : RequiredAttribute = null
            let mutable _defvalue : obj = null

            new() = ModelMetadata(null, Dictionary())

            member x.DataType           with get() = _dataType and set v = _dataType <- v
            member x.DisplayFormat      with get() = _displayFormat and set v = _displayFormat <- v
            member x.Display            with get() = _display  and set v = _display <- v
            member x.Editable           with get() = _editable and set v = _editable <- v
            member x.UIHint             with get() = _UIHint   and set v = _UIHint <- v
            member x.Required           with get() = _required and set v = _required <- v
            member x.DefaultValue       with get() = _defvalue and set v = _defvalue <- v

            member x.GetValue(model) = 
                null

            member x.GetPropertyMetadata (propertyInfo:PropertyInfo) = 
                properties.[propertyInfo]

        end

    type ModelValidationMetadata() = 
        class 
        end


    [<AbstractClass>]
    type ModelMetadataProvider() = 
        abstract member Create : ``type``:Type -> ModelMetadata

    [<AbstractClass>]
    type ModelValidationMetadataProvider() = 
        abstract member Create : ``type``:Type -> ModelValidationMetadata

    
    [<System.ComponentModel.Composition.Export(typeof<ModelMetadataProvider>)>]
    type DataAnnotationsModelMetadataProvider() = 
        inherit ModelMetadataProvider()

        let inspect_property (prop:PropertyInfo) = 
            let propMeta = ModelMetadata()
            propMeta.DisplayFormat <- read_att prop
            propMeta.Display       <- read_att prop
            propMeta.Editable      <- read_att prop
            propMeta.Required      <- read_att prop
            propMeta.UIHint        <- read_att_filter prop (fun f -> f.PresentationLayer = "MVC")
            let defVal = 
                let att : DefaultValueAttribute = read_att prop
                if att != null then att.Value else null
            propMeta.DefaultValue  <- defVal
            propMeta

        override x.Create(typ) =
            // MetadataTypeAttribute
            let dict = Dictionary() 
            
            typ.GetProperties( BindingFlags.Public ||| BindingFlags.Instance ) 
                |> Seq.map (fun p -> (p, (inspect_property p)) ) 
                |> Seq.iter (fun p -> dict.[fst p] <- snd p) 
                |> ignore

            // todo, build accessor lambda
            let meta = ModelMetadata(null, dict)
            meta


    [<System.ComponentModel.Composition.Export(typeof<ModelMetadataProvider>)>]
    type DataAnnotationsModelValidationMetadataProvider() = 
        inherit ModelValidationMetadataProvider()

        override x.Create(typ) =
            ModelValidationMetadata()

    

