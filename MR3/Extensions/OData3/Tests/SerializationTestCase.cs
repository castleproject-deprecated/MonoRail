using System;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	[TestFixture]
	public class SerializationTestCase : ODataTestCommon
	{
		private ODataMessageWriter _writer;

		[SetUp]
		public void Init()
		{
			
		}

		[Test]
		public void aaaaaaaaa()
		{
			var item = new ODataEntry();
			item.Id = "1";

			var edmModel = new EdmModel();
			var response = new StubODataResponse();
			var settings =
				CreateMessageWriterSettings(new Uri("http://localhost/something"), ODataFormat.Metadata);
			var writer = new ODataMessageWriter(response, settings, edmModel);

			var feedWriter = writer.CreateODataEntryWriter();
			feedWriter.WriteStart(item);
			feedWriter.WriteEnd();

			Console.WriteLine(response.ToString());

		}

	}
}
