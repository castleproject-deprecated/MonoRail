//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Extension.OData.Serialization

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Xml
open System.IO
open System.Text
open System.Xml
open Castle.MonoRail
open System.Data.OData
open System.Data.Services.Providers


type ResponseToSend = {
    mutable QItems : IQueryable;
    mutable SingleResult : obj;
    ResType : ResourceType;
    FinalResourceUri : Uri;
    ResProp : ResourceProperty;
    PropertiesToExpand : HashSet<ResourceProperty>;
}

[<AbstractClass;AllowNullLiteral>]
type Deserializer() = 
    class 
        abstract member DeserializeMany : rt:ResourceType * reader:TextReader * enc:Encoding -> IEnumerable
        abstract member DeserializeSingle : rt:ResourceType * reader:TextReader * enc:Encoding -> obj
    end

[<AbstractClass;AllowNullLiteral>]
type Serializer(wrapper:DataServiceMetadataProviderWrapper, serviceBaseUri:Uri, containerUri:Uri, rt:ResourceType, 
                propertiesToExpand:HashSet<ResourceProperty>, writer:TextWriter, enc:Encoding) = 

    abstract member SerializeMany : items:IEnumerable -> unit
    abstract member SerializeSingle : item:obj -> unit
    abstract member SerializeProperty : prop:ResourceProperty * value:obj -> unit

    member x.ShouldExpand (property:ResourceProperty) = propertiesToExpand.Contains property

    member x.Serialize (response:ResponseToSend) =
        let items : IQueryable = response.QItems
        let item = response.SingleResult
        let rt = response.ResType

        if items <> null then 
            x.SerializeMany (items)
        elif response.ResProp <> null && response.ResProp.IsOfKind(ResourcePropertyKind.Primitive) then 
            x.SerializeProperty (response.ResProp, item)
        else 
            x.SerializeSingle (item)


(*
[<AbstractClass;AllowNullLiteral>]
type SerializerStructure() = 
    class
        inherit Serializer() 

        member x.ShouldExpand (property:ResourceProperty) = 
            false
    
        member x.WriteItems () = 
            // ProcessSkipTop ?
            // ProcessSkipToken ?
            ()
        
        member x.WriteItem () = 
            ()
    
        member x.WriteProperty () = 
            ()
    
        member x.InternalWriteItems () = 
            ()
    
        member x.InternalWriteItem () = 
            ()

        member x.InternalProperty () = 
            ()

    end
*)



[<AutoOpen>]
module SerializerCommons = 
    begin

        type XmlReader with
            member x.ReadToElement() = 
                let doCont = ref true
                let isElement = ref false
                while !doCont do
                    match x.NodeType with 
                    | XmlNodeType.None | XmlNodeType.ProcessingInstruction 
                    | XmlNodeType.Comment | XmlNodeType.Whitespace 
                    | XmlNodeType.XmlDeclaration -> 
                        ()
                    | XmlNodeType.Text -> 
                        if String.IsNullOrEmpty x.Value || x.Value.Trim().Length <> 0 
                        then isElement := false; doCont := false
                    | XmlNodeType.Element -> 
                        isElement := true; doCont := false
                    | _ -> 
                        isElement := false; doCont := false
                    if !doCont then doCont := x.Read()
                !isElement

        type ResourceProperty
            with
                member x.GetValue(instance:obj) = 
                    let prop = instance.GetType().GetProperty(x.Name)
                    prop.GetValue(instance, null)
                member x.GetValueAsStr(instance:obj) = 
                    let prop = instance.GetType().GetProperty(x.Name)
                    let value = prop.GetValue(instance, null)
                    if value = null 
                    then null 
                    else value.ToString()
                member x.SetValue(instance:obj, value:obj) = 
                    let prop = instance.GetType().GetProperty(x.Name)
                    prop.SetValue(instance, value, null)
                    
        type ResourceType 
            with 
                member x.GetKey (instance:obj) = 
                    let keyValue = 
                        if x.KeyProperties.Count = 1 
                        then x.KeyProperties.[0].GetValueAsStr(instance)
                        else failwith "Composite keys are not supported"
                    sprintf "(%s)" keyValue
                (*
                member x.PathWithKey(instance:obj) = 
                    let keyValue = 
                        if x.KeyProperties.Count = 1 
                        then x.KeyProperties.[0].GetValueAsStr(instance)
                        else failwith "Composite keys are not supported"
                    sprintf "%s(%s)" x.Name keyValue
                *)

        let internal create_xmlwriter(writer:TextWriter) (encoding) = 
            let settings = XmlWriterSettings(CheckCharacters = false,
                                             ConformanceLevel = ConformanceLevel.Fragment,
                                             Encoding = encoding,
                                             Indent = true,
                                             NewLineHandling = NewLineHandling.Entitize)
            let xmlWriter = XmlWriter.Create(writer, settings)
            xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + encoding.WebName + "\" standalone=\"yes\"")
            xmlWriter

        let internal create_xmlreader(reader:TextReader) (encoding) = 
            let settings = XmlReaderSettings()
            XmlReader.Create(reader, settings)
    end

