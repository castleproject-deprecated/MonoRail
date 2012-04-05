namespace Castle.MonoRail.Extension.OData

open System
open System.Xml


type DeserializerFactory() = 

    member x.Create(contentType:string) = 
        match contentType.ToLowerInvariant() with
        | "application/atom+xml" -> 
            SyndicationSerialization.CreateDeserializer()
            
        | "application/json" -> 
            JSonSerialization.CreateDeserializer()
            
        | "application/xml"
        | "text/xml" -> 
            XmlSerialization.CreateDeserializer()
            
        | _ -> null