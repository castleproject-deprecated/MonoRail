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
	public class JSonSerializationTestCase : ODataTestCommon
	{
		protected EntitySerializer serializer;
		protected ODataMessageWriter writer;
		protected IEdmModel model;
		protected StubODataResponse response;

		[SetUp]
		public void Init()
		{
			var settings = CreateMessageWriterSettings(new Uri("http://testing/"), ODataFormat.JsonLight);
			model = BuildModel();
			response = new StubODataResponse();
			writer = new ODataMessageWriter(response, settings, model);
			serializer = new EntitySerializer(writer);
		}

		protected virtual IEdmModel BuildModel()
		{
			return Models.ModelWithAssociation.Build();
		}

		[Test]
		public void Deserialize_StandardJson_SimpleObj()
		{
			var rt = (IEdmEntityType) model.FindDeclaredType("schema.Product");

			var reader = new StringReader(
@"
{ Id: 1, Name: ""NewName"" }
");
			var result = new EntityDeserializer().ReadEntry(rt, reader);

			result.Should().NotBeNull();
			var repo = (Models.ModelWithAssociation.Product) result;
			repo.Id.Should().Be(1);
			repo.Name.Should().Be("NewName");
			repo.Categories.Should().BeNull();
		}
//
//		[Test]
//		public void Deserialize_VerboseJson_SimpleObj()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ d: { Id: 1, Name: ""NewName"" } }
//");
//
//			var result = JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//
//			result.Should().NotBeNull();
//			var repo = (SerializationModel.Repository)result;
//			repo.Id.Should().Be(1);
//			repo.Name.Should().Be("NewName");
//		}
//
//		[Test]
//		public void Deserialize_StandardJson_InnerComplexType()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ Id: 1, Name: ""NewName"", Info: { Ip: ""127.1.1.1"" } }
//");
//
//			var result = JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//
//			result.Should().NotBeNull();
//			var repo = (SerializationModel.Repository)result;
//			repo.Id.Should().Be(1);
//			repo.Name.Should().Be("NewName");
//			repo.Info.Ip.Should().Be("127.1.1.1");
//			repo.Branches.Should().HaveCount(0);
//		}
//
//		[Test]
//		public void Deserialize_StandardJson_InnerResourceRef()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ Id: 1, Name: ""NewName"", Branches: [ { Id: 2, Name: ""Branch1"", Owner: { Id : 3 } } ] }
//");
//
//			var result = JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//
//			result.Should().NotBeNull();
//			var repo = (SerializationModel.Repository)result;
//			repo.Id.Should().Be(1);
//			repo.Name.Should().Be("NewName");
//			repo.Branches.Should().HaveCount(1);
//			repo.Branches.ElementAt(0).Id.Should().Be(2);
//			repo.Branches.ElementAt(0).Name.Should().Be("Branch1");
//			repo.Branches.ElementAt(0).Owner.Should().NotBeNull();
//		}
//
//		[Test]
//		public void Deserialize_StandardJson_InnerResourceSet()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ Id: 1, Name: ""NewName"", Branches: [ { Id: 2, Name: ""Branch1"" }, { Id: 3, Name: ""Branch2"" } ] }
//");
//
//			var result = JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//
//			result.Should().NotBeNull();
//			var repo = (SerializationModel.Repository)result;
//			repo.Id.Should().Be(1);
//			repo.Name.Should().Be("NewName");
//			repo.Branches.Should().HaveCount(2);
//			repo.Branches.ElementAt(0).Id.Should().Be(2);
//			repo.Branches.ElementAt(1).Id.Should().Be(3);
//			repo.Branches.ElementAt(0).Name.Should().Be("Branch1");
//			repo.Branches.ElementAt(1).Name.Should().Be("Branch2");
//		}
//
//		[Test, ExpectedException(ExpectedMessage = "Property not found on model Branch: Revision")]
//		public void Deserialize_ReferenceToNonExistentProperty()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ 
//	Id: 1, 
//	Name: ""NewName"", 
//	Branches: [ 
//		{ 
//			Id: 2, 
//			Name: ""Branch1"", 
//			Revision: [ 
//				{ Id :100, FileName : ""Test1"" }, 
//				{ Id :101, FileName : ""Test2"" } 
//			] 
//		}, 
//		{ 
//			Id: 3, 
//			Name: ""Branch2"", 
//			Revision: [ 
//				{ Id :200, FileName : ""1Test1"" } 
//			] 
//		} 
//	] 
//}
//");
//
//			JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//		}
//
//		[Test]
//		public void Deserialize_StandardJson_InnerResourceSet_TwoLevels()
//		{
//			var rt = _model.GetResourceType("Repository").Value;
//
//			var reader = new StringReader(@"
//{ 
//	Id: 1, 
//	Name: ""NewName"", 
//	Branches: [ 
//		{ 
//			Id: 2, 
//			Name: ""Branch1"", 
//			Revisions: [ 
//				{ Id :100, FileName : ""Test1"" }, 
//				{ Id :101, FileName : ""Test2"" } 
//			] 
//		}, 
//		{ 
//			Id: 3, 
//			Name: ""Branch2"", 
//			Revisions: [ 
//				{ Id :200, FileName : ""1Test1"" } 
//			] 
//		} 
//	] 
//}
//");
//
//			var result = JSonSerialization.DeserializerInstance.DeserializeSingle(rt, reader, Encoding.UTF8, null);
//
//			result.Should().NotBeNull();
//			var repo = (SerializationModel.Repository)result;
//			repo.Id.Should().Be(1);
//			repo.Name.Should().Be("NewName");
// 			repo.Branches.Should().HaveCount(2);
//
//			var branch1 = repo.Branches.ElementAt(0);
//			var branch2 = repo.Branches.ElementAt(1);
//			branch1.Id.Should().Be(2);
//			branch2.Id.Should().Be(3);
//			branch1.Name.Should().Be("Branch1");
//			branch2.Name.Should().Be("Branch2");
//
//			branch1.Revisions.Should().HaveCount(2);
//			branch2.Revisions.Should().HaveCount(1);
//
//			branch1.Revisions.ElementAt(0).Id.Should().Be(100);
//			branch1.Revisions.ElementAt(1).Id.Should().Be(101);
//			branch1.Revisions.ElementAt(0).FileName.Should().Be("Test1");
//			branch1.Revisions.ElementAt(1).FileName.Should().Be("Test2");
//
//			branch2.Revisions.ElementAt(0).Id.Should().Be(200);
//			branch2.Revisions.ElementAt(0).FileName.Should().Be("1Test1");
//		}
	}
}
