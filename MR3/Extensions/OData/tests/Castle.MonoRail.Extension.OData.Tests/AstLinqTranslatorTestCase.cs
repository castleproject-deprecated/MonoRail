namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using System.Linq.Expressions;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class AstLinqTranslatorTestCase
	{
		private StubModel _model;
		private ResourceType _catalogRt;
		private List<Catalog2> _catalogs;

		[SetUp]
		public void Init()
		{
			_catalogs = new List<Catalog2>
			            	{
			            		new Catalog2() { Id = 1, Name = "catalog 1", Owner = new User2() { Email = "email1@m.com", Name = "Mary"} }, 
								new Catalog2() { Id = 2, Name = "catalog 2", Owner = new User2() { Email = "email2@m.com", Name = "John"} }, 
								new Catalog2() { Id = 3, Name = "catalog 3", Owner = new User2() { Email = "email3@m.com", Name = "Jeff"} }, 
								new Catalog2() { Id = 4, Name = "catalog 4", Owner = new User2() { Email = "email4@m.com", Name = "Andrew"} }, 
								new Catalog2() { Id = 5, Name = "catalog 5", Owner = new User2() { Email = "email5@m.com", Name = "Mary"} }, 
			            	};

			_model = new StubModel(
				m => m.EntitySet("catalogs", _catalogs.AsQueryable())
			);

			_catalogRt = _model.GetResourceType("Catalog2").Value;
		}

		private Expression<Func<T, bool>> BuildLinqExpression<T>(string expression, ResourceType rt)
		{
			var exp = QueryExpressionParser.parse(expression);
			// Console.WriteLine(exp4.ToStringTree());

			var tree = QuerySemanticAnalysis.analyze_and_convert(exp, rt);
			Console.WriteLine(tree.ToStringTree());
			
			return AstLinqTranslator.build_linq_exp_tree<T>(typeof(T), tree);
		}

		[Test]
		public void Property_Eq_String()
		{
			var exp = BuildLinqExpression<Catalog2>("Name eq 'catalog 1'", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(1);
			results.ElementAt(0).Name.Should().Be("catalog 1");
		}

		[Test]
		public void NestedProperty_Eq_String()
		{
			var exp = BuildLinqExpression<Catalog2>("Owner/Name eq 'Mary'", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(2);
			results.Where(c => c.Owner.Name == "Mary").Should().HaveCount(2);
		}

		[Test]
		public void Property_Eq_Int32()
		{
			var exp = BuildLinqExpression<Catalog2>("Id eq 1", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(1);
			results.ElementAt(0).Name.Should().Be("catalog 1");
		}

		[Test]
		public void BinaryNumericPromotion_Property_Eq_Int64()
		{
			var exp = BuildLinqExpression<Catalog2>("Id eq 1L", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(1);
			results.ElementAt(0).Name.Should().Be("catalog 1");
		}

		[Test]
		public void BinaryNumericPromotion_Property_Eq_Single()
		{
			var exp = BuildLinqExpression<Catalog2>("Id eq 1.0", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(1);
			results.ElementAt(0).Name.Should().Be("catalog 1");
		}

		[Test]
		public void Property_Ne_Int32()
		{
			var exp = BuildLinqExpression<Catalog2>("Id ne 1", _catalogRt);

			var results = _catalogs.Where(exp.Compile());
			results.Count().Should().Be(_catalogs.Count - 1);
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
