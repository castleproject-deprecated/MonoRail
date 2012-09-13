using System;
using System.Linq;
using Castle.MonoRail.OData.Internal;
using FluentAssertions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace Castle.MonoRail.Extension.OData3.Tests.Deserialization
{
	using System.IO;
	using System.Text;
	using NUnit.Framework;

	[TestFixture]
	public class JSonSerializationTestCase  
	{
		private StubODataResponse response;

		[SetUp]
		public void Init()
		{
			response = new StubODataResponse();
		}

		[Test]
		public void Deserialize_StandardJson_SimpleObj()
		{
			var model = Models.ModelWithAssociation.Build();
			var rt = (IEdmEntityType) model.FindDeclaredType("schema.Product");

			var reader = new StringReader(
@"
{ Id: 1, Name: ""NewName"" }
");
			var result = new EntityDeserializer().
				ReadEntry(new Models.ModelWithAssociation.Product(), 
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociation.Product) result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.Categories.Should().BeNull();
		}

		[Test]
		public void Deserialize_StandardJson_InnerComplexType()
		{
			var model = Models.ModelWithComplexType.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var reader = new StringReader(@"
{ Id: 1, Name: ""New Name"", MainAddress: { Name: ""Homeish"", City: ""Sao Paulo"", Zip: ""98711"" } }
");
			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithComplexType.Product(), 
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithComplexType.Product)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("New Name");
			repo.MainAddress.Should().NotBeNull();
			repo.MainAddress.Name.Should().Be("Homeish");
			repo.MainAddress.City.Should().Be("Sao Paulo");
			repo.MainAddress.Zip.Should().Be("98711");
			repo.OtherAddresses.Should().HaveCount(0);
		}

		[Test]
		public void Deserialize_StandardJson_CollOfComplexTypes()
		{
			var model = Models.ModelWithComplexType.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var reader = new StringReader(@"
{ Id: 1, Name: ""New Name"", OtherAddresses: [ { Name: ""Homeish"", City: ""Sao Paulo"", Zip: ""98711"" } ] }
");
			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithComplexType.Product(),
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithComplexType.Product)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("New Name");
			repo.MainAddress.Should().BeNull();
			repo.OtherAddresses.Should().HaveCount(1);

			var add = repo.OtherAddresses.ElementAt(0);
			add.Should().NotBeNull();
			add.Name.Should().Be("Homeish");
			add.City.Should().Be("Sao Paulo");
			add.Zip.Should().Be("98711");
		}

		[Test]
		public void Deserialize_StandardJson_InnerResourceRef()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var reader = new StringReader(@"
{ Id: 1, Name: ""NewName"", Categories: [ { Id: 2, Name: ""testing"" }, {  Id: 10, Name: ""testing again""  } ] }
");
			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithAssociationButSingleEntitySet.Product(), 
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociationButSingleEntitySet.Product)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.Categories.Should().NotBeNull();
			repo.Categories.ElementAt(0).Id.Should().Be(2);
			repo.Categories.ElementAt(0).Name.Should().Be("testing");

			repo.Categories.ElementAt(1).Id.Should().Be(10);
			repo.Categories.ElementAt(1).Name.Should().Be("testing again");
		}

		[Test]
		public void Deserialize_EmptyCollection()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var reader = new StringReader(@"
{ Id: 1, Name: ""NewName"", Categories: [  ] }
");
			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithAssociationButSingleEntitySet.Product(),
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociationButSingleEntitySet.Product)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.Categories.Should().NotBeNull();
			repo.Categories.Should().HaveCount(0);
		}

		[Test]
		public void Deserialize_NullCollection()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var reader = new StringReader(@"
{ Id: 1, Name: ""NewName"", Categories: null }
");
			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithAssociationButSingleEntitySet.Product(),
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociationButSingleEntitySet.Product)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.Categories.Should().NotBeNull();
			repo.Categories.Should().HaveCount(0);
		}

		[Test]
		public void Deserialize_Navigation_NullValue()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var reader = new StringReader(
@"{ Id: 1, Name: ""NewName"", ProductParent: null } ");

			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithAssociationButSingleEntitySet.Category(),
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociationButSingleEntitySet.Category)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.ProductParent.Should().BeNull();
		}

		[Test]
		public void Deserialize_Navigation_ValidRef()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			var rt = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var reader = new StringReader(
@"{ Id: 1, Name: ""NewName"", ProductParent: { Id : 102 } } ");

			var result = new EntityDeserializer().ReadEntry(
				new Models.ModelWithAssociationButSingleEntitySet.Category(),
				rt, reader, isMerge: false);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociationButSingleEntitySet.Category)result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.ProductParent.Should().NotBeNull();
			repo.ProductParent.Id.Should().Be(102);
		}

	}
}
