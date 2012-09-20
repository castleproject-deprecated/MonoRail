using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Serialization
{
	[TestFixture]
	public class EntitySerializer_EntrySupport_TestCase : EntitySerializerBase
	{
		[Test]
		public void WriteEntry_for_simple_model_writes_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Products");
			var element = new Models.ModelWithAssociation.Product() { Id = 1, Name = "Product Name" };
			var elType = entSet.ElementType;

			serializer.WriteEntry(entSet, element, elType);

			// Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Products/@Element"",""odata.id"":""testing"",""Categories@odata.navigationLinkUrl"":""http://testing/testing"",""Id"":1,""Name"":""Product Name""
}");
		}

		[Test]
		public void WriteEntry_for_model_with_onetomany_writes_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Products");
			var element = new Models.ModelWithAssociation.Product() { Id = 1, Name = "Product Name" };
			element.Categories = new List<Models.ModelWithAssociation.Category>
			{
				new Models.ModelWithAssociation.Category() { Id = 10, Name = "cat1" }, 
				new Models.ModelWithAssociation.Category() { Id = 11, Name = "cat2" }
			};
			var elType = entSet.ElementType;

			serializer.WriteEntry(entSet, element, elType);

			Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Products/@Element"",""odata.id"":""testing"",""Categories@odata.navigationLinkUrl"":""http://testing/testing"",""Id"":1,""Name"":""Product Name""
}");
		}

		[Test]
		public void WriteEntry_for_model_with_manytoone_writes_model()
		{
			var entSet = model.FindDeclaredEntityContainer("schema.container").FindEntitySet("Categories");
			var element = new Models.ModelWithAssociation.Category()
				              {
					              Id = 1, Name = "Category name",
								  ProductParent = new Models.ModelWithAssociation.Product() { Id = 100, Name = "iphone"}
				              };
			var elType = entSet.ElementType;

			serializer.WriteEntry(entSet, element, elType);

			Console.WriteLine(response);

			response.ToString().Should().Be(
@"DataServiceVersion 3.0;;Content-Type application/json;odata=light;streaming=true;charset=utf-8
{
  ""odata.metadata"":""http://testing/$metadata#schema.container/Categories/@Element"",""odata.id"":""testing"",""ProductParent@odata.navigationLinkUrl"":""http://testing/testing"",""Parent@odata.navigationLinkUrl"":""http://testing/testing"",""Id"":1,""Name"":""Category name""
}");
		}
	}
}
