using System;
using System.Collections.Generic;
using Castle.MonoRail.OData.Internal;
using FluentAssertions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Serialization
{
	[TestFixture]
	public class EntitySerializer_EntrySupport2_TestCase : EntitySerializerBase
	{
		protected override IEdmModel BuildModel()
		{
			return Models.ModelWithComplexType.Build();
		}

		[Test]
		public void WriteEntry_for_model_with_complextype_writes_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Products");
			var element = new Models.ModelWithComplexType.Product()
				              {
					              Id = 1, Name = "Product Name", 
								  MainAddress = new Models.ModelWithComplexType.Address() { Name = "test"}
				              };
			var elType = entSet.ElementType;

			serializer.WriteEntry(entSet, element, elType);

			// Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Products/@Element"",""odata.id"":""testing"",""Id"":1,""Name"":""Product Name"",""MainAddress"":{
    ""Name"":""test"",""City"":null,""Zip"":null
  },""OtherAddresses"":[
    
  ]
}");
		}

		[Test]
		public void WriteEntry_for_model_with_complextypecollection_writes_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Products");
			var element = new Models.ModelWithComplexType.Product()
			{
				Id = 1,
				Name = "Product Name",
				OtherAddresses = new List<Models.ModelWithComplexType.Address>
					                 {
						                 new Models.ModelWithComplexType.Address() { Name = "test1" },
										 new Models.ModelWithComplexType.Address() { Name = "test2" }
					                 }
			};
			var elType = entSet.ElementType;

			serializer.WriteEntry(entSet, element, elType);

			// Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Products/@Element"",""odata.id"":""testing"",""Id"":1,""Name"":""Product Name"",""MainAddress"":null,""OtherAddresses"":[
    {
      ""Name"":""test1"",""City"":null,""Zip"":null
    },{
      ""Name"":""test2"",""City"":null,""Zip"":null
    }
  ]
}");
		}

	}
}
