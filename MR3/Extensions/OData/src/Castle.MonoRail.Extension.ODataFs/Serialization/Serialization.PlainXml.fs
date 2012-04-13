namespace Castle.MonoRail.Extension.OData

open System
open System.Xml

module XmlSerialization = 
    begin
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
                override x.SerializeSingle(wrapper, svcBaseUri, containerUri, rt, item, writer, enc) = 
                    // write_item wrapper svcBaseUri containerUri rt item writer enc 
                    raise(NotImplementedException())
            }

    end