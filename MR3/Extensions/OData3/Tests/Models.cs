using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Castle.MonoRail.Extension.OData3.Tests
{
    using System;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Expressions;

    public static class Models
	{
		public class ExtraTypes
		{
			public class SearchResult
			{
				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
			} 
		}

		public class SimpleODataModel : ODataModel
		{
			public SimpleODataModel() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>().AsQueryable());
			}

			public class Product
			{
				[Key]
				public int Id { get; set; }

				public string Name { get; set; }
			}

            public static IEdmModel Build()
            {
                var odata = new SimpleODataModel();
                odata.Initialize();
                return 
                    EdmModelBuilder.build("schema", "container",
                        odata.EntitiesConfigs, new Type[0],
                            (t, m) => Enumerable.Empty<IEdmFunctionImport>());
            }

            public static IEdmModel BuildWithFunctions()
            {
                var odata = new SimpleODataModel();
                odata.Initialize();
                return
                    EdmModelBuilder.build("schema", "container",
                        odata.EntitiesConfigs, 
                        new Type[0], GetProductFunctions());
            }

            // DescriptiveName(Product prod) : string
            // Reviews(Product prod) : Collection<Reviews>
            // Top(Products prods) : Collection<Product>
            public static Func<Type, IEdmModel, IEnumerable<IEdmFunctionImport>> GetProductFunctions()
            {
                return (type, model) =>
                {
                    var productSet = model.EntityContainers().Single().FindEntitySet("Products");

                    return new IEdmFunctionImport[]
                    {
                        new StubEdmFunctionImport()
                            {
                                Name = "DescriptiveName",
                                EntitySet = new EdmEntitySetReferenceExpression(productSet),
                                IsBindable = true,
                                ReturnType = EdmCoreModel.Instance.GetString(true),
                                IsSideEffecting = false,
                                Parameters = new List<IEdmFunctionParameter>
                                {
                                    new StubEdmFunctionParameter()
                                    {
                                        Name = "prod", 
                                        Type = new EdmEntityTypeReference(productSet.ElementType, false)
                                    }
                                }
                            },
                        new StubEdmFunctionImport(){
                                Name = "Reviews",
                                EntitySet = new EdmEntitySetReferenceExpression(productSet),
                                IsBindable = true,
                                // should be complex type: Review
                                ReturnType = EdmCoreModel.GetCollection(new EdmEntityTypeReference(productSet.ElementType, false)),
                                IsSideEffecting = false,
                                Parameters = new List<IEdmFunctionParameter>
                                {
                                    new StubEdmFunctionParameter()
                                    {
                                        Name = "prod", 
                                        Type = new EdmEntityTypeReference(productSet.ElementType, false)
                                    }
                                }
                            }, 
                        new StubEdmFunctionImport(){
                                Name = "Top",
                                EntitySet = new EdmEntitySetReferenceExpression(productSet),
                                IsBindable = true,
                                ReturnType = EdmCoreModel.GetCollection(new EdmEntityTypeReference(productSet.ElementType, false)),
                                IsSideEffecting = false,
                                Parameters = new List<IEdmFunctionParameter>
                                {
                                    new StubEdmFunctionParameter()
                                    {
                                        Name = "prod", Type = new EdmEntityTypeReference(productSet.ElementType, false)
                                    },
                                    new StubEdmFunctionParameter()
                                    {
                                        Name = "count", Type = EdmCoreModel.Instance.GetInt32(false)
                                    }
                                }
                            }, 
                    };
                };
            }
		}

		public class ModelWithAssociation : ODataModel
		{
			public ModelWithAssociation() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>().AsQueryable());
				this.EntitySet("Categories", new List<Category>().AsQueryable());
			}

			public class Product
			{
				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
				public IList<Category> Categories { get; set; }
			}

			public class Category
			{
				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
				public Product ProductParent { get; set; }
				public Category Parent { get; set; }
			}
		}
	}
}
