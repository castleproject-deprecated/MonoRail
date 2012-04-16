namespace Castle.MonoRail.Extension.OData

open System
open System.Xml
open System.Data.Services.Providers

module XmlSerialization = 
    begin

        let internal write_primitive_value (rt:ResourceType) (prop:ResourceProperty) value (writer:XmlWriter) = 
            let name = prop.Name

            writer.WriteStartElement (name, "http://schemas.microsoft.com/ado/2007/08/dataservices")

            let typename = rt.FullName

            if typename <> "Edm.String" then 
                writer.WriteAttributeString ("type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", typename)
            
            if value = null 
            then writer.WriteAttributeString("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true")
            else writer.WriteString(value.ToString())            

            writer.WriteEndElement()

        let private write_primitive (rt:ResourceType) (prop:ResourceProperty) (value:obj) (txtwriter) (enc) = 
            use writer = SerializerCommons.create_xmlwriter (txtwriter) enc
            write_primitive_value rt prop value writer


        let CreateDeserializer () = 
            { new Deserializer() with 
                override x.DeserializeMany (rt, reader, enc) = 
                    // read_feed rt reader enc
                    raise(NotImplementedException())
                override x.DeserializeSingle (rt, reader, enc) = 
                    // read_item rt reader enc
                    raise(NotImplementedException())
            }

        let CreateSerializer () = 
            { new Serializer() with 
                override x.SerializeMany(wrapper, svcBaseUri, containerUri , rt, items, writer, enc) = 
                    // write_items wrapper svcBaseUri containerUri rt items writer enc
                    raise(NotImplementedException()) 

                override x.SerializeSingle(wrapper, svcBaseUri, containerUri , rt, items, writer, enc) = 
                    ()

                override x.SerializePrimitive(wrapper, svcBaseUri, containerUri, rt, prop, value, writer, enc) = 
                    // write_item wrapper svcBaseUri containerUri rt item writer enc 
                    // raise(NotImplementedException())
                    write_primitive rt prop value writer enc
            }

    end