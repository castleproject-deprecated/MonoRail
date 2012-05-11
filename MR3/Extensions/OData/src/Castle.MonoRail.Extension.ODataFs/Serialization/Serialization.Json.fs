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
open System.ServiceModel.Syndication
open System.Data.OData
open System.Data.Services.Providers
open System.Data.Services.Common
open Newtonsoft.Json



module JSonSerialization = 
    begin
        let private set_up (writer:JsonTextWriter) = 
            writer.DateFormatHandling <- DateFormatHandling.IsoDateFormat
            writer.Formatting <- Formatting.Indented
            writer.IndentChar <- '\t'
            writer.Indentation <- 1


        type JsonSerializer(wrapper, serviceBaseUri, containerUri, rt, propertiesToExpand, writer, enc, useSimplerFormat:bool) as self = 
            class 
                inherit Serializer(wrapper, serviceBaseUri, containerUri, rt, propertiesToExpand, writer, enc) 

                let jwriter = new JsonTextWriter(writer)
                
                do
                    set_up jwriter

                let write_meta (uri:Uri) (rt:ResourceType) = 
                    if not useSimplerFormat then
                        jwriter.WritePropertyName "__metadata"
                        jwriter.WriteStartObject()
                        if uri <> null then
                            jwriter.WritePropertyName "uri"
                            jwriter.WriteValue uri.AbsoluteUri
                        jwriter.WritePropertyName "type"
                        jwriter.WriteValue rt.FullName
                        jwriter.WriteEndObject()

                let rec write_primitive_and_complex_properties (instance) (uri:Uri) (rt:ResourceType) = 
                    
                    for prop in rt.Properties do
                        let otherRt = prop.ResourceType
                
                        if prop.IsOfKind ResourcePropertyKind.ComplexType then
                            // todo: add case for collection of complex types

                            jwriter.WritePropertyName prop.Name

                            // TODO: is collection?
                            // if prop.ResourceType.InstanceType.IsColl then 

                            let innerinstance = prop.GetValue(instance)

                            write_complextype innerinstance uri otherRt

                        elif prop.IsOfKind ResourcePropertyKind.Primitive then
                    
                            jwriter.WritePropertyName prop.Name

                            let originalVal = prop.GetValue(instance)
                    
                            jwriter.WriteValue originalVal


                and write_ref_properties (instance) (uri:Uri) (rt:ResourceType) = 
            
                    for prop in rt.Properties do
                        let otherRt = prop.ResourceType

                        if prop.IsOfKind ResourcePropertyKind.ResourceReference || prop.IsOfKind ResourcePropertyKind.ResourceSetReference then
                            jwriter.WritePropertyName prop.Name
                            if not useSimplerFormat then jwriter.WriteStartObject ()

                            // spec wise, we need to output additional metadata in the end (after the properties) 
                            // to reference the associations used for the expanded properties, but I'm skipping that for now

                            if self.ShouldExpand (prop) then
                                if prop.IsOfKind ResourcePropertyKind.ResourceSetReference then 
                                    if not useSimplerFormat then jwriter.WritePropertyName "results" 

                                    let innerItems = prop.GetValue(instance) :?> IEnumerable
                                    if innerItems <> null then
                                        write_set (Uri(uri.AbsoluteUri + "/" + prop.Name)) prop.ResourceType innerItems true 
                                    else
                                        jwriter.WriteStartArray ()
                                        jwriter.WriteEndArray ()
                                else
                                    if not useSimplerFormat then jwriter.WritePropertyName "result" 

                                    let inner = prop.GetValue(instance) 
                                    if inner <> null then
                                        write_js_item inner (Uri(uri.AbsoluteUri + "/" + prop.Name)) prop.ResourceType true 
                                    else
                                        jwriter.WriteNull()

                            else
                                if not useSimplerFormat then 
                                    jwriter.WritePropertyName "__deferred"
                                    jwriter.WriteStartObject ()
                                    jwriter.WritePropertyName "uri"
                                    jwriter.WriteValue (uri.AbsoluteUri + "/" + prop.Name)
                                    jwriter.WriteEndObject ()
                                else
                                    jwriter.WriteStartObject ()
                                    jwriter.WriteEndObject ()
                                

                            if not useSimplerFormat then jwriter.WriteEndObject ()


                and write_complextype (instance) (uri:Uri) (rt:ResourceType) = 
        
                    if instance = null then 
                        jwriter.WriteNull()
                    else
                        jwriter.WriteStartObject()
                        write_meta null rt
                        write_primitive_and_complex_properties instance uri rt
                        jwriter.WriteEndObject()


                and write_js_item (instance) (containerUri:Uri) (rt:ResourceType) appendKey = 
                    jwriter.WriteStartObject()

                    let resourceSet = wrapper.ResourceSets |> Seq.tryFind (fun rs -> rs.ResourceType = rt)
                    let resourceUri = 
                        match resourceSet with 
                        | Some rs -> 
                            // for this case, we always want to append the key
                            Uri(serviceBaseUri, rs.Name + rt.GetKey(instance))
                        | _ -> 
                            System.Diagnostics.Debug.Assert (containerUri <> null)
                            if appendKey 
                            then Uri(containerUri.AbsoluteUri + rt.GetKey(instance))
                            else containerUri
            
                    write_meta resourceUri rt
                    write_primitive_and_complex_properties instance resourceUri rt 
                    write_ref_properties instance resourceUri rt 

                    jwriter.WriteEndObject()

                and write_set (containerUri:Uri) (rt:ResourceType) (items:IEnumerable) appendKey = 
            
                    jwriter.WriteStartArray()

                    for item in items do
                        write_js_item item containerUri rt appendKey 

                    jwriter.WriteEndArray()

                let wrap_in_d (f) = 
                    if not useSimplerFormat then 
                        jwriter.WriteStartObject()
                        jwriter.WritePropertyName "d"

                    f()
            
                    if not useSimplerFormat then 
                        jwriter.WriteEndObject()

                override x.SerializeMany(items) =
                    wrap_in_d (fun _ -> write_set containerUri rt items true )

                override x.SerializeSingle(item) =
                    wrap_in_d (fun _ -> write_js_item item containerUri rt false  )
                
                override x.SerializeProperty(prop:ResourceProperty, value) =
                    
                    let write_d () = 
                        jwriter.WriteStartObject()
                        jwriter.WritePropertyName prop.Name
                        jwriter.WriteValue value
                        jwriter.WriteEndObject() 

                    wrap_in_d (fun _ -> write_d ()  )

            end
        


        let internal read_item (rt:ResourceType) (reader:TextReader) (enc:Encoding) = 
            
            use jsonReader = new JsonTextReader(reader)
            let instance = Activator.CreateInstance rt.InstanceType

            // { "d": { Prop: a, Prop2: 2 } }
            // { Prop: a, Prop2: 2 }

            let getToPropertyStart () = 
                let doContinue = ref true
                while !doContinue && jsonReader.Read() do
                    if jsonReader.TokenType = JsonToken.PropertyName && jsonReader.Value.ToString() <> "d" then
                        doContinue := false
                
            getToPropertyStart()

            let doContinue = ref true
            while !doContinue do
                if jsonReader.TokenType = JsonToken.PropertyName then 
                    match rt.Properties |> Seq.tryFind (fun p -> p.Name = jsonReader.Value.ToString()) with
                    | Some prop -> 
                        jsonReader.Read() |> ignore
                        // todo: assert is not comment or property name

                        let value = jsonReader.Value

                        if prop.IsOfKind (ResourcePropertyKind.Primitive) then 
                            
                            let sanitizedVal = Convert.ChangeType(value, prop.ResourceType.InstanceType)

                            prop.SetValue(instance, sanitizedVal)

                        elif prop.IsOfKind (ResourcePropertyKind.ComplexType) then 
                            
                            ()
                        else 
                            ()

                        doContinue := jsonReader.Read()

                    | _ ->  
                        // could not find property: should this be an error?
                        doContinue := false

                else
                    doContinue := jsonReader.Read()

            instance


        let CreateDeserializer = 
            { 
              new Deserializer() with 
                override x.DeserializeMany (rt, reader, enc) = 
                    raise(NotImplementedException())
                override x.DeserializeSingle (rt, reader, enc) = 
                    read_item rt reader enc
            }

    end
