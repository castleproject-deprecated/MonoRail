namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Common;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
	using System.Text;
	using NUnit.Framework;
	using Castle.MonoRail.Extension.OData.Serialization;

	[TestFixture]
	public class AtomServiceDocSerializerTestCase
	{
		[Test]
		public void OnlyWritesEntitySets_()
		{
			var model = new StubModel(m =>
			{
				m.EntitySet("products", new List<Product2>().AsQueryable())
					.AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, true));
				m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
				m.EntitySet("suppliers", new List<Supplier2>().AsQueryable());
			});
			var writer = new StringWriter();

			AtomServiceDocSerializer.serialize(writer, new Uri("http://localhost/app"),  new DataServiceMetadataProviderWrapper(model), Encoding.UTF8);

			Console.WriteLine(writer.GetStringBuilder().ToString());
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
	}
}
