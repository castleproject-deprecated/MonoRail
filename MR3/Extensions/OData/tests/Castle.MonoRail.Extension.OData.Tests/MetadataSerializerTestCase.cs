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

		[Test]
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

		[Test]
		public void ModelWithSimpleUnidirectionalRelationship_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("Products", new List<Product3>().AsQueryable());
				m.EntitySet("Vendors", new List<Vendor3>().AsQueryable());
					
			});
			var writer = new StringWriter();

			MetadataSerializer.serialize(writer, new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);
			
			Console.WriteLine(writer.GetStringBuilder().ToString());
		}

		[Test]
		public void ModelWithSimpleBidirectionalRelationship_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("Products", new List<Product4>().AsQueryable());
				m.EntitySet("Vendors", new List<Vendor4>().AsQueryable());

			});
			var writer = new StringWriter();

			MetadataSerializer.serialize(writer, new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);

			Console.WriteLine(writer.GetStringBuilder().ToString());
		}

		[Test]
		public void ModelWithRelationship_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("products", new List<Product2>().AsQueryable())
					.AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, true));
				m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
				m.EntitySet("suppliers", new List<Supplier2>().AsQueryable());
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


		[Test]
		public void ModelWithRelationshipOnResourceTypes_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
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

		// -------------------------------------

		public class Address2
		{
			public string Street { get; set; }
			public string Zip { get; set; }
			public string Country { get; set; }
		}

		public class Supplier2
		{
			[Key]
			public int Id { get; set; }
			public Address2 Address { get; set; }
		}

		public class Catalog2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product2> Products { get; set; }
		}
		public class Product2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public Catalog2 Catalog { get; set; }
		}

		// -------------------------------------

		public class Vendor3
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class Product3
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public Vendor3 Vendor { get; set; }
		}

		// -------------------------------------

		public class Vendor4
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product4> Products { get; set; }
		}

		public class Product4
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public Vendor4 Vendor { get; set; }
		}
	}
}
