namespace Castle.MonoRail.Tests.Serializers
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using NUnit.Framework;
	using Serialization;

	[TestFixture]
	public class JsonDeserializationTestCase
	{
		[Test]
		public void Single()
		{
			var json = "{credentials: {cpf: 289, dob: '2013-05-17T18:12:38.0116622-03:00', password:123456}}";

			var serializer = (IModelSerializer<Credentials>) new JsonSerializer<Credentials>(null);

			var context = new ModelSerializationContext(new MemoryStream(Encoding.UTF8.GetBytes(json)), new NameValueCollection());

			var instance = serializer.Deserialize("credentials", MediaTypes.JSon, context, new DataAnnotationsModelMetadataProvider());

			Assert.IsNotNull(instance);
			Assert.AreEqual("289", instance.cpf);
		}

		[Test]
		public void Composite()
		{
			var json = @"{
							credentials: {cpf: 289, dob: '2013-05-17T18:12:38.0116622-03:00', password:123456},
							order: {symbol: 'PETR4', price: 1.01},
						 }";

			var context = new ModelSerializationContext(new MemoryStream(Encoding.UTF8.GetBytes(json)), new NameValueCollection());

			var credentailsSerializer = (IModelSerializer<Credentials>) new JsonSerializer<Credentials>(null);
			var credentials = credentailsSerializer.Deserialize("credentials", MediaTypes.JSon, context, new DataAnnotationsModelMetadataProvider());
			Assert.IsNotNull(credentials);
			Assert.AreEqual("289", credentials.cpf);

			var orderSerializer = (IModelSerializer<Order>) new JsonSerializer<Order>(null);
			var order = orderSerializer.Deserialize("order", MediaTypes.JSon, context, new DataAnnotationsModelMetadataProvider());
			Assert.IsNotNull(order);
			Assert.AreEqual("PETR4", order.Symbol);

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