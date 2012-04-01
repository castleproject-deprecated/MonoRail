namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.ComponentModel.DataAnnotations;
	using System.Collections.Generic;
	using System.Data.Services.Common;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
	using System.Text;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class MetadataSerializerTestCase
	{
		[Test]
		public void EmptySet_()
		{
			var model = new StubModel(null);
			var writer = new StringWriter();

			MetadataSerializer.serialize(writer, new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);

			Console.WriteLine(writer.GetStringBuilder().ToString());

//			writer.GetStringBuilder().ToString().Should().Be(
//@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
//<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
//  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""2.0"" />
//</edmx:Edmx>");
		}

		public void SimpleModel_()
		{
			var model = new StubModel(m => m.EntitySet("catalogs", new List<Catalog1>().AsQueryable()));
			var writer = new StringWriter();

			MetadataSerializer.serialize(writer, new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);

//			writer.GetStringBuilder().ToString().Should().Be(
//@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
//<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
//  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""2.0"" />
//</edmx:Edmx>");

			Console.WriteLine(writer.GetStringBuilder().ToString());
		}

		public void ModelWithRelationship_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
				m.EntitySet("products", new List<Product2>().AsQueryable())
					.AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, true)); 
			});
			var writer = new StringWriter();

			MetadataSerializer.serialize(writer, new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);

//			writer.GetStringBuilder().ToString().Should().Be(
//@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
//<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
//  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""2.0"" />
//</edmx:Edmx>");

			Console.WriteLine(writer.GetStringBuilder().ToString());
		}

		public class Catalog1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}


		public class Catalog2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IEnumerable<Product2> Products { get; set; }
		}
		public class Product2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }	
		}
	}
}
