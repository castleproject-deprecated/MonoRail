namespace Castle.MonoRail.Mvc.Rest
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Export(typeof(FormatSerializer))]
    [ExportMetadata("MimeTypes", "application/xml")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class XmlFormatSerializer : FormatSerializer
    {
        public override void Serialize(object data, Stream output)
        {
            // what if data is null?

            var serializer = new DataContractSerializer(data.GetType());
            serializer.WriteObject(output, data);
        }

        public override object Deserialize(Type modelType, Stream input)
        {
            var serializer = new DataContractSerializer(modelType);
            return serializer.ReadObject(input);
        }
    }
}