using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Castle.MonoRail.OData;
using Castle.MonoRail.OData.Internal;
using FluentAssertions;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	[TestFixture]
	public class EdmModelBuilderTestCase
	{
		[Test]
		public void build_with_empty_configs_generates_empty_model()
		{
			var model = 
				EdmModelBuilder.build("ns", "container1", 
					Enumerable.Empty<EntitySetConfig>(), 
					Enumerable.Empty<Type>(), (t, m) => Enumerable.Empty<IEdmFunctionImport>());

			model.Should().NotBeNull();
			model.EntityContainers().Should().HaveCount(1);
			model.FindEntityContainer("container1").Elements.Should().HaveCount(0);
		}

		[Test]
		public void build_with_simple_config_generates_container_with_entity_type_and_set()
		{
			var config = new EntitySetConfig("Products", "Product", new List<object>().AsQueryable(), typeof(Models.SimpleODataModel.Product));

			var model =
				EdmModelBuilder.build("ns", "container1",
					new EntitySetConfig[] { config }, 
					Enumerable.Empty<Type>(), (t, m) => Enumerable.Empty<IEdmFunctionImport>());

			model.Should().NotBeNull();
			model.SchemaElements.Should().HaveCount(2);
			model.FindDeclaredType("ns.Product").Should().NotBeNull();
			model.EntityContainers().Should().HaveCount(1);
			model.FindEntityContainer("container1").Elements.Should().HaveCount(1);
			model.FindEntityContainer("container1").FindEntitySet("Products").Should().NotBeNull();

			var typeDef = (IEdmEntityType) model.FindDeclaredType("ns.Product");
			typeDef.TypeKind.Should().Be(EdmTypeKind.Entity);
			typeDef.Properties().Should().HaveCount(2);

			typeDef.DeclaredKey.Should().HaveCount(1);
			typeDef.DeclaredKey.First().Name.Should().Be("Id");

			typeDef.Properties().ElementAt(0).Name.Should().Be("Id");
			typeDef.Properties().ElementAt(1).Name.Should().Be("Name");

			typeDef.Properties().ElementAt(0).PropertyKind.Should().Be(EdmPropertyKind.Structural);
			typeDef.Properties().ElementAt(1).PropertyKind.Should().Be(EdmPropertyKind.Structural);

			typeDef.Properties().ElementAt(0).Type.FullName().Should().Be("Edm.Int32");
			typeDef.Properties().ElementAt(1).Type.FullName().Should().Be("Edm.String");
		}

		[Test]
		public void build_with_extra_types_generates_types()
		{
			var config = new EntitySetConfig("Products", "Product", new List<object>().AsQueryable(), typeof(Models.SimpleODataModel.Product));

			var model =
				EdmModelBuilder.build("ns", "container1",
					new EntitySetConfig[] { config },
					new[] { typeof(Models.ExtraTypes.SearchResult) },
                    (t, m) => Enumerable.Empty<IEdmFunctionImport>());

			model.Should().NotBeNull();
			model.SchemaElements.Should().HaveCount(3);
			model.FindDeclaredType("ns.Product").Should().NotBeNull();
			model.EntityContainers().Should().HaveCount(1);
			model.FindEntityContainer("container1").Elements.Should().HaveCount(1);
			model.FindEntityContainer("container1").FindEntitySet("Products").Should().NotBeNull();

			var typeDef = (IEdmEntityType)model.FindDeclaredType("ns.SearchResult");
			typeDef.TypeKind.Should().Be(EdmTypeKind.Entity);
			typeDef.Properties().Should().HaveCount(2);

			typeDef.DeclaredKey.Should().HaveCount(1);
			typeDef.DeclaredKey.First().Name.Should().Be("Id");

			typeDef.Properties().ElementAt(0).Name.Should().Be("Id");
			typeDef.Properties().ElementAt(1).Name.Should().Be("Name");

			typeDef.Properties().ElementAt(0).PropertyKind.Should().Be(EdmPropertyKind.Structural);
			typeDef.Properties().ElementAt(1).PropertyKind.Should().Be(EdmPropertyKind.Structural);

			typeDef.Properties().ElementAt(0).Type.FullName().Should().Be("Edm.Int32");
			typeDef.Properties().ElementAt(1).Type.FullName().Should().Be("Edm.String");
		}

		[Test]
		public void build_with_function_resolver()
		{
			var config = new EntitySetConfig("Products", "Product", 
				new List<object>().AsQueryable(), typeof(Models.SimpleODataModel.Product));

			var model =
				EdmModelBuilder.build("ns", "container1",
					new [] { config },
					Enumerable.Empty<Type>(),
                    (t, m) =>
						{
							if (t == typeof(Models.SimpleODataModel.Product))
							{
								return new[] { new StubEdmFunctionImport() { Name = "Cheapest" }, };
							}
							return Enumerable.Empty<IEdmFunctionImport>();
						});

			model.Should().NotBeNull();
			model.SchemaElements.Should().HaveCount(2);
			var container = model.EntityContainers().Single();

			var functions = container.FindFunctionImports("Cheapest");
			functions.Should().NotBeNull();
			functions.Should().HaveCount(1);

			var function = functions.Single();
			function.Should().NotBeNull();
		}
	}
}
