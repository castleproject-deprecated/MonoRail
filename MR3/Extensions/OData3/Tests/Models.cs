using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Castle.MonoRail.Extension.OData3.Tests
{
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
