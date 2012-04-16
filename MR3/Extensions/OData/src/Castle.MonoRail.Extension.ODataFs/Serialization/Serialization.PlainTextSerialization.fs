namespace Castle.MonoRail.Extension.OData

open System
open System.IO
open System.Xml
open System.Data.Services.Providers

module PlainTextSerialization = 
    begin

        let internal write_primitive_value (rt:ResourceType) (prop:ResourceProperty) value (writer:TextWriter) = 
            if value = null 
            then writer.Write "null"
            else writer.Write (value.ToString())            

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
                    raise(NotImplementedException()) 

                override x.SerializePrimitive(wrapper, svcBaseUri, containerUri, rt, prop, value, writer, enc) = 
                    write_primitive_value rt prop value writer 
            }

    end