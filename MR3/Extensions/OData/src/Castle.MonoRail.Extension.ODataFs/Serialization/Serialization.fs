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


[<AbstractClass;AllowNullLiteral>]
type Deserializer() = 
    class 
    end

[<AbstractClass;AllowNullLiteral>]
type Serializer() = 
    class 

        abstract member SerializeMany : baseUri:Uri * rt:ResourceType * items:IEnumerable * writer:TextWriter * enc:Encoding -> unit
        abstract member SerializeSingle : baseUri:Uri * rt:ResourceType * items:obj * writer:TextWriter * enc:Encoding -> unit

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
    end