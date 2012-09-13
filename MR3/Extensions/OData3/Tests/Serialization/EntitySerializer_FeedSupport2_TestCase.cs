using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Serialization
{
	[TestFixture]
	public class EntitySerializer_FeedSupport2_TestCase : EntitySerializerBase
	{
		protected override IEdmModel BuildModel()
		{
			return Models.ModelWithAssociationButSingleEntitySet.Build();
		}

		[Test]
		public void WriteFeed_for_a_simple_model()
		{
			var elements = new []
				               {
					               new Models.ModelWithAssociationButSingleEntitySet.Category() {Id = 1, Name = "Cat1"}
				               };
			var elType = model.FindDeclaredType("schema.Category");
			var edmType = (IEdmEntityType) elType;

			serializer.WriteFeed(
				new EdmEntitySet(model.FindEntityContainer("schema.container"), "Category", edmType), 
				elements.AsQueryable(), edmType );

			// Console.WriteLine(response);
			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Category"",""value"":[
    {
      ""odata.id"":""testing"",""ProductParent@odata.navigationLinkUrl"":""http://testing/testing"",""ProductParent"":null,""Parent@odata.navigationLinkUrl"":""http://testing/testing"",""Parent"":null,""Id"":1,""Name"":""Cat1""
    }
  ]
}");
		}
	}
}