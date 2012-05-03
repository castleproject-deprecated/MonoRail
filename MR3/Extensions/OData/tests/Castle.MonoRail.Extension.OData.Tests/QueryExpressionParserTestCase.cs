namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class QueryExpressionParserTestCase
	{
		[Test]
		public void JustRunning()
		{
			var model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog2>().AsQueryable())
			);

			// filter = Customers/ContactName ne 'Fred'
//			var exp0 = QueryExpressionParser.parse("1 add 2 mul 3");
//			Console.WriteLine(exp0.ToStringTree());

			var exp1 = QueryExpressionParser.parse("Customers ne 'Fred'");
			Console.WriteLine(exp1.ToStringTree());

//			var exp2 = QueryExpressionParser.parse("Customers/N ne 'Fred'");
//			Console.WriteLine(exp2.ToStringTree());
//
//			var exp3 = QueryExpressionParser.parse("Customers/Address/Street ne 'Fred'");
//			Console.WriteLine(exp3.ToStringTree());

			// QuerySemanticAnalysis.analyze(exp1, model.GetResourceType("Catalog2").Value);
		}

		public class Product2
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
			public IList<Product2> Products { get; set; }
		}
	}
}
