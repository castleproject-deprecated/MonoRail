namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Data.Services.Providers;
	using System.Linq;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ODataMetadataBuilderTestCase
	{
		[Test]
		public void SingleEntityWith0PropsWithKey_BuildsResource()
		{
			var model = new StubModel(t =>
			                          	{
			                          		t.SchemaNamespace = "ns";
											t.EntitySet("name", new List<EntWithKey>().AsQueryable(), EntitySetPermission.ReadOnly);
			                          	});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			var resType = result.ElementAt(0);
			resType.Should().NotBeNull();
			resType.Name.Should().Be("name");
			resType.Namespace.Should().Be("ns");
			resType.ResourceTypeKind.Should().Be(ResourceTypeKind.EntityType);
			resType.Properties.Count.Should().Be(1);
			resType.Properties.ElementAt(0).Kind.Should().Be(ResourcePropertyKind.Primitive | ResourcePropertyKind.Key);
			resType.Properties.ElementAt(0).Name.Should().Be("Id");
		}

		[Test]
		public void SingleEntityWithPrimitiveProperties_BuildsResource()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("name", new List<EntWithPropsKey>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			var resType = result.ElementAt(0);
			resType.Should().NotBeNull();
			resType.Name.Should().Be("name");
			resType.Namespace.Should().Be("ns");
			resType.ResourceTypeKind.Should().Be(ResourceTypeKind.EntityType);
			resType.Properties.Count.Should().Be(4);

			var id = resType.Properties.FirstOrDefault(p => p.Name == "Id");
			id.Kind.Should().Be(ResourcePropertyKind.Primitive | ResourcePropertyKind.Key);
			id.ResourceType.InstanceType.Should().Be<int>();
			
			var name = resType.Properties.FirstOrDefault(p => p.Name == "Name");
			name.Kind.Should().Be(ResourcePropertyKind.Primitive);
			name.ResourceType.InstanceType.Should().Be<string>();
			
			var age = resType.Properties.FirstOrDefault(p => p.Name == "Age");
			age.Kind.Should().Be(ResourcePropertyKind.Primitive);
			age.ResourceType.InstanceType.Should().Be<int>();
			
			var dob = resType.Properties.FirstOrDefault(p => p.Name == "DoB");
			dob.Kind.Should().Be(ResourcePropertyKind.Primitive);
			dob.ResourceType.InstanceType.Should().Be<DateTime>();
		}

		[Test]
		public void SingleEntityWithComplexTypeProperty_BuildsResource()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("name", new List<EntWithComplexPropKey>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			var resType = result.ElementAt(0);
			resType.Should().NotBeNull();
			resType.Name.Should().Be("name");
			resType.Namespace.Should().Be("ns");
			resType.ResourceTypeKind.Should().Be(ResourceTypeKind.EntityType);
			resType.Properties.Count.Should().Be(2);
			
			var id = resType.Properties.FirstOrDefault(p => p.Name == "Id");
			id.Kind.Should().Be(ResourcePropertyKind.Primitive | ResourcePropertyKind.Key);
			id.ResourceType.InstanceType.Should().Be<int>();

			var address = resType.Properties.FirstOrDefault(p => p.Name == "AddressProp");
			address.Kind.Should().Be(ResourcePropertyKind.ComplexType);
			address.ResourceType.InstanceType.Should().Be<Address>();

			address.ResourceType.Properties.Count.Should().Be(2);
			var address1 = address.ResourceType.Properties.FirstOrDefault(p => p.Name == "Address1");
			address1.Should().NotBeNull();
			address1.Kind.Should().Be(ResourcePropertyKind.Primitive);
			address1.ResourceType.InstanceType.Should().Be<string>();

			var city = address.ResourceType.Properties.FirstOrDefault(p => p.Name == "City");
			city.Should().NotBeNull();
			city.Kind.Should().Be(ResourcePropertyKind.Primitive);
			city.ResourceType.InstanceType.Should().Be<string>();
		}


		[Test]
		public void TwoEntitiesNotRelated_BuildsTwoResources()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("name1", new List<EntWithKey>().AsQueryable(), EntitySetPermission.ReadOnly);
				t.EntitySet("name2", new List<EntWithPropsKey>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			result.Count().Should().Be(2);
		}

		[Test]
		public void TwoEntitiesRelatedByDirectResourceRef_CardinalityOne_BuildsTwoResources()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("product", new List<Product1>().AsQueryable(), EntitySetPermission.ReadOnly);
				t.EntitySet("supplier", new List<Supplier1>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			result.Count().Should().Be(2);

			var productRT = result.FirstOrDefault(rt => rt.Name == "product");
			productRT.Should().NotBeNull();
			var supplierRT = result.FirstOrDefault(rt => rt.Name == "supplier");
			supplierRT.Should().NotBeNull();
			
			var supplierProp = productRT.Properties.FirstOrDefault(p => p.Name == "Supplier");
			supplierProp.Should().NotBeNull();
			supplierProp.Kind.Should().Be(ResourcePropertyKind.ResourceReference);
			supplierProp.ResourceType.Should().BeSameAs(supplierRT);
		}

		[Test]
		public void TwoEntitiesRelatedByDirectResourceRef_CardinalityMany_BuildsTwoResources()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("product", new List<Product2>().AsQueryable(), EntitySetPermission.ReadOnly);
				t.EntitySet("supplier", new List<Supplier2>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			result.Count().Should().Be(2);

			var productRT = result.FirstOrDefault(rt => rt.Name == "product");
			productRT.Should().NotBeNull();
			var supplierRT = result.FirstOrDefault(rt => rt.Name == "supplier");
			supplierRT.Should().NotBeNull();

			var productsProp = supplierRT.Properties.FirstOrDefault(p => p.Name == "Products");
			productsProp.Should().NotBeNull();
			productsProp.Kind.Should().Be(ResourcePropertyKind.ResourceSetReference);
			productsProp.ResourceType.Should().BeSameAs(productRT);
		}

		[Test]
		public void TwoEntitiesReferencingEachOther_CardinalityOneAndMany_BuildsTwoResources()
		{
			var model = new StubModel(t =>
			{
				t.SchemaNamespace = "ns";
				t.EntitySet("product", new List<Product3>().AsQueryable(), EntitySetPermission.ReadOnly);
				t.EntitySet("supplier", new List<Supplier3>().AsQueryable(), EntitySetPermission.ReadOnly);
			});
			var builder = new ODataMetadataBuilder(model);

			var result = builder.Build();

			result.Should().NotBeNull();
			result.Count().Should().Be(2);

			var productRT = result.FirstOrDefault(rt => rt.Name == "product");
			productRT.Should().NotBeNull();
			var supplierRT = result.FirstOrDefault(rt => rt.Name == "supplier");
			supplierRT.Should().NotBeNull();

			var productsProp = supplierRT.Properties.FirstOrDefault(p => p.Name == "Products");
			productsProp.Should().NotBeNull();
			productsProp.Kind.Should().Be(ResourcePropertyKind.ResourceSetReference);
			productsProp.ResourceType.Should().BeSameAs(productRT);
		}


		public class EntWithKey
		{
			[Key]
			public int Id { get; set; }
		}

		public class EntWithPropsKey
		{
			[Key]
			public int Id { get; set; }

			public string Name { get; set; }
			public int Age { get; set; }
			public DateTime DoB { get; set; }
		}

		public class Address
		{
			public string Address1 { get; set; }
			public string City { get; set; }
		}

		public class EntWithComplexPropKey
		{
			[Key]
			public int Id { get; set; }

			public Address AddressProp { get; set; }
		}

		public class Supplier1
		{
			[Key]
			public int Id { get; set; }
		}

		public class Product1
		{
			[Key]
			public int Id { get; set; }
			public Supplier1 Supplier { get; set; }
		}

		public class Supplier2
		{
			[Key]
			public int Id { get; set; }
			public IList<Product2> Products { get; set; }
		}

		public class Product2
		{
			[Key]
			public int Id { get; set; }
		}

		public class Supplier3
		{
			[Key]
			public int Id { get; set; }
			public IList<Product3> Products { get; set; }
		}

		public class Product3
		{
			[Key]
			public int Id { get; set; }
			public Supplier3 Supplier { get; set; }
		}
	}

	class StubModel : ODataModel
	{
		public StubModel(Action<ODataModel> modelFn)
		{
			if (modelFn != null)
			{
				modelFn(this);
			}
		}
	}
}
