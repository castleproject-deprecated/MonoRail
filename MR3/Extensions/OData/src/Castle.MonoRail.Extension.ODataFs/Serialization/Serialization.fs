namespace Castle.MonoRail.Extension.OData

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
    mutable EItems : IEnumerable;
    mutable SingleResult : obj;
    ResType : ResourceType;
    FinalResourceUri : Uri;
    ResProp : ResourceProperty;
}

[<AbstractClass;AllowNullLiteral>]
type Deserializer() = 
    class 
        abstract member DeserializeMany : rt:ResourceType * reader:TextReader * enc:Encoding -> IEnumerable
        abstract member DeserializeSingle : rt:ResourceType * reader:TextReader * enc:Encoding -> obj
    end


[<AbstractClass;AllowNullLiteral>]
type Serializer() = 
    abstract member SerializeMany : wrapper:DataServiceMetadataProviderWrapper * serviceBaseUri:Uri * containerUri:Uri * rt:ResourceType * items:IEnumerable * writer:TextWriter * enc:Encoding -> unit
    abstract member SerializeSingle : wrapper:DataServiceMetadataProviderWrapper * serviceBaseUri:Uri * containerUri:Uri * rt:ResourceType * items:obj * writer:TextWriter * enc:Encoding -> unit
    abstract member SerializePrimitive : wrapper:DataServiceMetadataProviderWrapper * serviceBaseUri:Uri * containerUri:Uri * rt:ResourceType * prop:ResourceProperty * value:obj * writer:TextWriter * enc:Encoding -> unit

    member x.Serialize (response:ResponseToSend, wrapper:DataServiceMetadataProviderWrapper, serviceBaseUri:Uri, containerUri:Uri, writer:TextWriter, enc:Encoding) =
        let items : IEnumerable = 
            if response.QItems <> null 
            then upcast response.QItems 
            else response.EItems
        let item = response.SingleResult
        let rt = response.ResType

        if items <> null then 
            x.SerializeMany (wrapper, serviceBaseUri, containerUri, rt, items, writer, enc)
        elif response.ResProp <> null && response.ResProp.IsOfKind(ResourcePropertyKind.Primitive) then 
            x.SerializePrimitive (wrapper, serviceBaseUri, containerUri, rt, response.ResProp, item, writer, enc)
        else 
            x.SerializeSingle (wrapper, serviceBaseUri, containerUri, rt, item, writer, enc)


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

        type System.Data.Services.Providers.ResourceProperty
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
                    
        type System.Data.Services.Providers.ResourceType 
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

