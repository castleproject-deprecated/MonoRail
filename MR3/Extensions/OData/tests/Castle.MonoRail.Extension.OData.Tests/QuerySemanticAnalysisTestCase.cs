namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class QuerySemanticAnalysisTestCase
	{
		private StubModel _model;

		[SetUp]
		public void Init()
		{
			_model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog2>().AsQueryable())
			);
		}

		private QueryAst AnalyzeAndConvert(string expression, ResourceType rt)
		{
			var exp = QueryExpressionParser.parse(expression);
			// Console.WriteLine(exp4.ToStringTree());

			var tree = QuerySemanticAnalysis.analyze_and_convert(exp, rt);
			Console.WriteLine(tree.ToStringTree());
			return tree;
		}

		[Test]
		public void Binary_Eq_ForInt32s()
		{
			var tree = AnalyzeAndConvert("1 add 2 eq 3", _model.GetResourceType("Catalog2").Value);

		}

		[Test]
		public void Binary_Eq_ForDecimals()
		{
			var tree = AnalyzeAndConvert("1.2 add 2.3 eq 3.5", _model.GetResourceType("Catalog2").Value);

		}

		[Test]
		public void PropNavigation()
		{
			var tree = AnalyzeAndConvert("Owner/Email eq 'ema@mail.com'", _model.GetResourceType("Catalog2").Value);

		}

		public class Product2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class User2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
		}

		public class Catalog2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product2> Products { get; set; }
			public User2 Owner { get; set; }
		}
	}
}
