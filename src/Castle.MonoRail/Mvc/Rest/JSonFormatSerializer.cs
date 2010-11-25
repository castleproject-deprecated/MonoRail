namespace Castle.MonoRail.Mvc.Rest
{
	using System;
	using System.ComponentModel.Composition;
	using System.IO;
	using System.Runtime.Serialization.Json;

	[Export(typeof(FormatSerializer))]
	[ExportMetadata("MimeTypes", new [] { "application/json", "application/x-javascript" })]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class JSonFormatSerializer : FormatSerializer
	{
		public override void Serialize(object data, Stream output)
		{
			// what if data is null?

			var serializer = new DataContractJsonSerializer(data.GetType());
			serializer.WriteObject(output, data);
		}

		public override object Deserialize(Type modelType, Stream input)
		{
			var serializer = new DataContractJsonSerializer(modelType);
			return serializer.ReadObject(input);
		}
	}
}