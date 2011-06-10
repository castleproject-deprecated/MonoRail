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

namespace Castle.MonoRail.Serialization

    open System
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail


    [<Interface>]
    type public IModelSerializer<'a> = 
        abstract member Serialize : model:'a * contentType:string * writer:System.IO.TextWriter -> unit
        abstract member Deserialize : prefix:string * contentType:string * request:HttpRequestBase -> 'a


    type JsonSerializer<'a>() = 
        
        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                // very inneficient for large models
                let content = Newtonsoft.Json.JsonConvert.SerializeObject(model, new Newtonsoft.Json.Converters.IsoDateTimeConverter())
                writer.Write content

            member x.Deserialize (prefix, contentType, request) = 
                // very inneficient for large inputs
                let reader = new StreamReader(request.InputStream)
                let content = reader.ReadToEnd()
                Newtonsoft.Json.JsonConvert.DeserializeObject<'a>(content)


    type XmlSerializer<'a>() = 

        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                let serial = System.Runtime.Serialization.DataContractSerializer(typeof<'a>)
                let memStream = new MemoryStream()
                serial.WriteObject (memStream, model)
                let en = System.Text.UTF8Encoding()
                let content = en.GetString (memStream.GetBuffer(), 0, int(memStream.Length))
                writer.Write content

            member x.Deserialize (prefix, contentType, request) = 
                let serial = System.Runtime.Serialization.DataContractSerializer(typeof<'a>)
                let graph = serial.ReadObject( request.InputStream )
                graph :?> 'a


    type FormBasedSerializer<'a>() = 

        interface IModelSerializer<'a> with
            member x.Serialize (model:'a, contentType:string, writer:System.IO.TextWriter) = 
                ()

            member x.Deserialize (prefix, contentType, request) = 
                Activator.CreateInstance typeof<'a> :?> 'a
                // Unchecked.defaultof<'a>


    [<Export()>]
    type ModelSerializerResolver() = 
        //                           model, list mime*Serializer
        let _custom = lazy Dictionary<Type,List<MimeType*Type>>()
        let _defSerializers = lazy (
                                        let dict = Dictionary<MimeType,Type>()
                                        dict.Add (MimeType.JSon, typedefof<JsonSerializer<_>>)
                                        dict.Add (MimeType.Xml, typedefof<XmlSerializer<_>>)
                                        dict.Add (MimeType.FormUrlEncoded, typedefof<FormBasedSerializer<_>>)
                                        dict
                                    )

        member x.Register<'a>(mime:MimeType, serializer:Type) = 
            let modelType = typeof<'a>
            let dict = _custom.Force()
            let exists,list = dict.TryGetValue modelType
            if not exists then
                let list = List<_>()
                list.Add (mime,serializer)
                dict.[modelType] <- list
            else
                list.Add (mime,serializer)


        // memoization would be a good thing here, since serializers should be stateless
        member x.CreateSerializer<'a>(mime:MimeType) : IModelSerializer<'a> = 
            let mutable serializerType : Type = null
            if _custom.IsValueCreated then
                failwith "not implemented"

            if serializerType == null then
                let dict = _defSerializers.Force()
                let exists, tmpType = dict.TryGetValue mime
                if exists then
                    serializerType <- tmpType

            if serializerType != null then
                if serializerType.IsGenericTypeDefinition then
                    let instantiatedType = serializerType.MakeGenericType( [|typeof<'a>|] )
                    Activator.CreateInstance instantiatedType :?> IModelSerializer<'a>
                else
                    Activator.CreateInstance serializerType :?> IModelSerializer<'a>
            else 
                Unchecked.defaultof<_>

                
