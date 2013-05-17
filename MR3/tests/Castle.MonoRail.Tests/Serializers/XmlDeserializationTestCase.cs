namespace Castle.MonoRail.Tests.Serializers
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using NUnit.Framework;
	using Serialization;

	[TestFixture]
	public class XmlDeserializationTestCase
	{
		[Test]
		public void Simple_Deserialization()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<credentials><cpf>248</cpf><dob>1900-01-01</dob><password>123456</password></credentials>
";
			var serializer = (IModelSerializer<Credentials>) new XmlSerializer<Credentials>();

			var context = new ModelSerializationContext(new MemoryStream(Encoding.UTF8.GetBytes(xml)), new NameValueCollection());

			var instance = serializer.Deserialize("credentials", MediaTypes.Xml, context, new DataAnnotationsModelMetadataProvider());

			Assert.IsNotNull(instance);
			Assert.AreEqual("248", instance.cpf);

		}

		[Test]
		public void Composite_Deserilization()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<envelope>
	<credentials><cpf>248</cpf><dob>1900-01-01</dob><password>123456</password></credentials>
	<order><Symbol>MR3</Symbol><Price>3</Price></order>
</envelope>
";
			var context = new ModelSerializationContext(new MemoryStream(Encoding.UTF8.GetBytes(xml)), new NameValueCollection());

			var credentials = (IModelSerializer<Credentials>) new XmlSerializer<Credentials>();
			var c = credentials.Deserialize("credentials", MediaTypes.Xml, context, new DataAnnotationsModelMetadataProvider());
			Assert.IsNotNull(c);
			Assert.AreEqual("248", c.cpf);

			var order = (IModelSerializer<Order>) new XmlSerializer<Order>();
			var o = order.Deserialize("order", MediaTypes.Xml, context, new DataAnnotationsModelMetadataProvider());
			Assert.IsNotNull(o);
			Assert.AreEqual("MR3", o.Symbol);
		}

		public class Credentials
		{
			public string cpf;
			public string password;
			public DateTime dob;
		}

		public class Order
		{
			public string Symbol { get; set; }

			public decimal Price { get; set; }
		}
	}
}