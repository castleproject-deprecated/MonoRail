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
    class 
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

            ()

            // let prop = result

    end


module SerializerCommons = 
    begin
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