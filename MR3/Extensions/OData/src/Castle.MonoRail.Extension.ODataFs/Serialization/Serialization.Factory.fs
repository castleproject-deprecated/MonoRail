namespace Castle.MonoRail.Extension.OData

open System
open System.Xml


type SerializerFactory() = 

    static member Create(contentType:string) : Serializer = 

        match contentType.ToLowerInvariant() with
        | "application/atom+xml" -> 
            AtomSerialization.CreateSerializer()
            
        | "application/json" -> 
            JSonSerialization.CreateSerializer()
            
        | "text/xml"  
        | "application/xml" ->
            XmlSerialization.CreateSerializer()

        | "text/plain" ->
            PlainTextSerialization.CreateSerializer()
            
        | _ -> failwithf "unsupported content type %s" contentType



type DeserializerFactory() = 

    static member Create(contentType:string) : Deserializer = 

        match contentType.ToLowerInvariant() with
        | "application/atom+xml" -> 
            AtomSerialization.CreateDeserializer()
            
        | "application/json" -> 
            JSonSerialization.CreateDeserializer()
            
        | "application/xml"
        | "text/xml" -> 
            XmlSerialization.CreateDeserializer()
            
        | _ -> failwithf "unsupported content type %s" contentType