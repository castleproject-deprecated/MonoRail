using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Serialization
{
	[TestFixture]
	public class EntitySerializer_FeedSupport_TestCase : EntitySerializerBase
	{
		[Test]
		public void WriteFeed_for_a_simple_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Products");
			var elements = new []
				               {
					               new Models.ModelWithAssociation.Product() {Id = 1, Name = "Product Name"}
				               };
			var elType = entSet.ElementType;

			serializer.WriteFeed(entSet, elements.AsQueryable(), elType);

			//Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Products"",""value"":[
    {
      ""odata.id"":""testing"",""Categories@odata.navigationLinkUrl"":""http://testing/testing"",""Id"":1,""Name"":""Product Name""
    }
  ]
}");
		}
	}
}