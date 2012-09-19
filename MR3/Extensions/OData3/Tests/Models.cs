using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Castle.MonoRail.OData.Internal;

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
						new StubEdmFunctionImport()
							{
								Name = "Best",
								EntitySet = new EdmEntitySetReferenceExpression(productSet),
								IsBindable = true,
								ReturnType = new EdmEntityTypeReference(productSet.ElementType, false),
								IsSideEffecting = false,
								Parameters = new List<IEdmFunctionParameter>
								{
									new StubEdmFunctionParameter()
									{
										Name = "prods", 
										Type = EdmCoreModel.GetCollection(new EdmEntityTypeReference(productSet.ElementType, false))
									}
								}
							},
						new StubEdmFunctionImport()
							{
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
						new StubEdmFunctionImport()
							{
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
			public ModelWithAssociation() : base("schemaNs", "containerName")
			{
				
			}

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>()
					                           {
						                           new Product { Id = 1, Name = "prod 1" },
												   new Product { Id = 2, Name = "prod 2" },
					                           }.AsQueryable());
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

			public static IEdmModel Build()
			{
				var odata = new ModelWithAssociation();
				odata.Initialize();
				return
					EdmModelBuilder.build("schema", "container",
						odata.EntitiesConfigs, new Type[0],
							(t, m) => Enumerable.Empty<IEdmFunctionImport>());
			}
		}

		public class ModelWithComplexType : ODataModel
		{
			public ModelWithComplexType() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>().AsQueryable());
			}

			public class Product
			{
				public Product()
				{
					this.OtherAddresses = new List<Address>();
				}

				[Key]
				public int Id { get; set; }
				public string Name { get; set; }

				public Address MainAddress { get; set; }
				public IList<Address> OtherAddresses { get; set; }
			}

			public class Address
			{
				public string Name { get; set; }
				public string City { get; set; }
				public string Zip { get; set; }
			}

			public static IEdmModel Build()
			{
				var odata = new ModelWithComplexType();
				odata.Initialize();
				return 
					EdmModelBuilder.build("schema", "container",
						odata.EntitiesConfigs, 
						new Type[0],
						(t, m) => Enumerable.Empty<IEdmFunctionImport>());
			}
		}

        public class ModelWithEnums : ODataModel
        {
            public ModelWithEnums() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>().AsQueryable());
			}

            public static IEdmModel Build()
            {
                var odata = new ModelWithEnums();
                odata.Initialize();
                return
                    EdmModelBuilder.build("schema", "container",
                        odata.EntitiesConfigs,
                        new Type[0],
                        (t, m) => Enumerable.Empty<IEdmFunctionImport>());
            }

            public enum StatusType
            {
                InStock,
                PreOrder = 2,
                BackOrder = 5
            }

			public class Product
			{
				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
                public StatusType Status { get; set; }
			}
        }

		public class ModelWithAssociationButSingleEntitySet : ODataModel
		{
			public ModelWithAssociationButSingleEntitySet() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				this.EntitySet("Products", new List<Product>().AsQueryable());
			}

			public class Product
			{
				public Product()
				{
					this.Categories = new List<Category>();
				}

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

			public static IEdmModel Build()
			{
				var odata = new ModelWithAssociationButSingleEntitySet();
				odata.Initialize();
				return
					EdmModelBuilder.build("schema", "container",
						odata.EntitiesConfigs, new Type[0],
							(t, m) => Enumerable.Empty<IEdmFunctionImport>());
			}
		}

		public class ModelWithIndirection : ODataModel
		{
			public ModelWithIndirection() : base("schemaNs", "containerName") { }

			public override void Initialize()
			{
				EntitySet("Products", new List<Product>().AsQueryable())
					.ForProperty<IList<MongoRef<Category>>, IList<Category>>(
						p => p.Categories,
						getter: f => f.Select(e => new Category() { Id = e.Id } ).ToList(),
						setter: v => v.Select(e => new MongoRef<Category>(e.Id)).ToList())
					.ForProperty<MongoRef<Category>, Category>(
						p => new Category().Parent,
						getter: f => new Category() { Id = f.Id },
						setter: v => new MongoRef<Category>(v.Id));
			}

			public class MongoRef<T>
			{
				private int _id;

				public MongoRef(int id)
				{
					_id = id;
				}

				public int Id { get { return _id; } }
			}

			public class Product
			{
				public Product()
				{
					this.Categories = new List<MongoRef<Category>>();
				}

				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
				public IList<MongoRef<Category>> Categories { get; set; }
			}

			public class Category
			{
				[Key]
				public int Id { get; set; }
				public string Name { get; set; }
				public MongoRef<Category> Parent { get; set; }
			}

			public static IEdmModel Build()
			{
				var odata = new ModelWithIndirection();
				odata.Initialize();
				return
					EdmModelBuilder.build("schema", "container",
						odata.EntitiesConfigs, new Type[0],
							(t, m) => Enumerable.Empty<IEdmFunctionImport>());
			}
		}

	}
}
