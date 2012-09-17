//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using FluentAssertions;
	using Microsoft.Data.Edm;
	using MonoRail.OData.Internal;
	using NUnit.Framework;
	using OData3.Tests;

	[TestFixture]
	public class AstLinqTranslatorTestCase
	{
		private IEdmEntityType _prodEdmType;
		private IQueryable<Models.ModelWithAssociationButSingleEntitySet.Product> _products;
		
		private IEdmEntityType _prodWithComplexEdmType;
		private IQueryable<Models.ModelWithComplexType.Product> _productsWComplex;

		[SetUp]
		public void Init()
		{
			var model = Models.ModelWithAssociationButSingleEntitySet.Build();
			_prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			_products = new List<Models.ModelWithAssociationButSingleEntitySet.Product>
				            {
					            new Models.ModelWithAssociationButSingleEntitySet.Product() { Id = 1, Name = "product 1"},
								new Models.ModelWithAssociationButSingleEntitySet.Product() { Id = 2, Name = "product 2"},
								new Models.ModelWithAssociationButSingleEntitySet.Product() { Id = 3, Name = "product 3"},
								new Models.ModelWithAssociationButSingleEntitySet.Product() { Id = 4, Name = "product 4"},
								new Models.ModelWithAssociationButSingleEntitySet.Product() { Id = 5, Name = "product 5"},
				            }.AsQueryable();
			
			var modelWComplex = Models.ModelWithComplexType.Build();
			_prodWithComplexEdmType = (IEdmEntityType)modelWComplex.FindDeclaredType("schema.Product");
			_productsWComplex = new List<Models.ModelWithComplexType.Product>
				            {
					            new Models.ModelWithComplexType.Product() { Id = 1, Name = "product 1"},
								new Models.ModelWithComplexType.Product() { Id = 2, Name = "product 2"},
								new Models.ModelWithComplexType.Product() { Id = 3, Name = "product 3"},
								new Models.ModelWithComplexType.Product() { Id = 4, Name = "product 4"},
								new Models.ModelWithComplexType.Product() { Id = 5, Name = "product 5"},
				            }.AsQueryable();
		}

		[Test]
		public void filter_by_non_existent_property_value_yields_zero_items()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithAssociationButSingleEntitySet.Product>("Name eq 'blah blah'", _prodEdmType);

			var results = _products.Where(exp.Compile());
			results.Count().Should().Be(0);
		}

		[Test]
		public void BinaryPropValue_filter_by_existent_property_value_yields_one_item()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithAssociationButSingleEntitySet.Product>("Name eq 'product 1'", _prodEdmType);

			var results = _products.Where(exp.Compile());
			results.Count().Should().Be(1);
			results.ElementAt(0).Name.Should().Be("product 1");
		}

		[Test]
		// why Linq is not implementing a short circuit here?!?!
		public void NestedProperty_Eq_String()
		{
			foreach (var product in _productsWComplex)
			{
				product.MainAddress = new Models.ModelWithComplexType.Address();
			}

			_productsWComplex.ElementAt(0).MainAddress = new Models.ModelWithComplexType.Address() { Name = "Home" };
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>(
				"MainAddress ne null and MainAddress/Name eq 'Home'", _prodWithComplexEdmType);

			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(1);

			_productsWComplex.Where(c => c.MainAddress != null && c.MainAddress.Name == "Home").Should().HaveCount(1);
		}

		[Test]
		public void Property_Ne_NegateOfSingle()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id ne -1.0", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(_productsWComplex.Count());
		}

		[Test]
		public void UnaryExp_Property_Eq_Not()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("not (Id eq 1)", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(_productsWComplex.Count() - 1);
		}

		[Test]
		public void Property_GreaterThan()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id gt 1", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(_productsWComplex.Count() - 1);
		}

		[Test]
		public void Property_GreaterEqualThan()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id ge 1", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(_productsWComplex.Count());
		}

		[Test]
		public void Property_LessThan()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id lt 1", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(0);
		}

		[Test]
		public void Property_LessEqualThan()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id le 1", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(1);
		}

		[Test]
		public void Property_GreaterThan_And_Eq()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id gt 1 and Name eq 'product 2'", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(1);
		}

		[Test]
		public void Property_GreaterThan_Or_Eq()
		{
			var exp = BuildLinqExpressionPredicate<Models.ModelWithComplexType.Product>("Id eq 2 or Name eq 'product 1'", _prodWithComplexEdmType);
			var results = _productsWComplex.Where(exp.Compile());
			results.Count().Should().Be(2);
		}

		private Expression<Func<T, bool>> BuildLinqExpressionPredicate<T>(string expression, IEdmType rt)
		{
			var exp = QueryExpressionParser.parse_filter(expression);
			// Console.WriteLine(exp4.ToStringTree());

			var tree = QuerySemanticAnalysis.analyze_and_convert(exp, rt);
			//Console.WriteLine(tree.ToStringTree());
			
			return AstLinqTranslator.build_linq_exp_predicate<T>(typeof(T), tree);
		}

//		private static IQueryable<T> ApplyOrderByExpression<T>(IQueryable<T> source, string expression, ResourceType rt)
//		{
//			var exp = QueryExpressionParser.parse_orderby(expression);
//			// Console.WriteLine(exp4.ToStringTree());
//			var tree = QuerySemanticAnalysis.analyze_and_convert_orderby(exp, rt);
//			// Console.WriteLine(tree.ToStringTree());
//			return AstLinqTranslator.typed_queryable_orderby<T>(source, tree) as IQueryable<T>;
//		}
//
//		[Test]
//		public void OrderBy_Asc_StringProperty()
//		{
//			var result = ApplyOrderByExpression(_catalogs.AsQueryable(), "Name", _catalogRt);
//			result.Should().NotBeNull();
//			var names = result.Select(c => c.Name);
//			var expected = _catalogs.OrderBy(c => c.Name).Select(c => c.Name);
//			names.Should().Equal(expected);
//		}
//
//		[Test]
//		public void OrderBy_Desc_StringProperty()
//		{
//			var result = ApplyOrderByExpression(_catalogs.AsQueryable(), "Name desc", _catalogRt);
//			result.Should().NotBeNull();
//			var names = result.Select(c => c.Name);
//			var expected = _catalogs.OrderByDescending(c => c.Name).Select(c => c.Name);
//			names.Should().Equal(expected);
//		}
//
//		[Test]
//		public void OrderBy_Asc_Int32Property()
//		{
//			var result = ApplyOrderByExpression(_catalogs.AsQueryable(), "Id", _catalogRt);
//			result.Should().NotBeNull();
//			var resultElems = result.Select(c => c.Id);
//			var expected = _catalogs.OrderBy(c => c.Id).Select(c => c.Id);
//			resultElems.Should().Equal(expected);
//		}
//
//		[Test]
//		public void OrderBy_Desc_Int32Property()
//		{
//			var result = ApplyOrderByExpression(_catalogs.AsQueryable(), "Id desc", _catalogRt);
//			result.Should().NotBeNull();
//			var resultElems = result.Select(c => c.Id);
//			var expected = _catalogs.OrderByDescending(c => c.Id).Select(c => c.Id);
//			resultElems.Should().Equal(expected);
//		}
//
//		[Test]
//		public void OrderBy_Asc_StringProperty_AndThen_Desc_Int32Property()
//		{
//			var result = ApplyOrderByExpression(_catalogs.AsQueryable(), "Name, Id desc", _catalogRt);
//			result.Should().NotBeNull();
//			var names = result.Select(c => c.Name);
//			var expected = _catalogs.OrderBy(c => c.Name).ThenByDescending(c => c.Id).Select(c => c.Name);
//			names.Should().Equal(expected);
//		}
//

//		private readonly string[] numericOps = new[] { "sub", "add", "mul", "div", "mod" };
//		private readonly string[] relationalOps = new[] { "lt", "gt", "le", "ge" };
//		private readonly string[] eqOps = new[] { "eq", "ne" };
//		private readonly string[][] numericTypes = new[]
//		                                           	{
//		                                           		new [] { "Edm.Int32", "2", "PropInt32" },
//														new [] { "Edm.Int64", "3L", "PropInt64" },
//														new [] { "Edm.Decimal", "4m", "PropDec" },
//														new [] { "Edm.Double", "5.0D", "PropSingle" },
//														new [] { "Edm.Single", "6F", "PropDouble" },
//		                                           	};
//
//		[Test]
//		public void EqualityOps_On_NumericTypes_Properties()
//		{
//			foreach (var op in eqOps)
//				foreach (var left in numericTypes)
//					foreach (var right in numericTypes)
//					{
//						var rawEx = left[2] + " " + op + " " + right[2];
//						var exp = BuildLinqExpressionPredicate<Catalog2>(rawEx, _catalogRt);
//						var results = _catalogs.Where(exp.Compile());
//						results.Count();
//					}
//		}
//
//		[Test]
//		public void RelationalsOps_On_NumericTypes_Properties()
//		{
//			foreach (var op in relationalOps)
//				foreach (var left in numericTypes)
//					foreach (var right in numericTypes)
//					{
//						var rawEx = left[2] + " " + op + " " + right[2];
//						var exp = BuildLinqExpressionPredicate<Catalog2>(rawEx, _catalogRt);
//						var results = _catalogs.Where(exp.Compile());
//						results.Count();
//					}
//		}
//
//		[Test]
//		public void NumericOps_On_NumericTypes_Properties()
//		{
//			foreach (var op in numericOps)
//				foreach (var left in numericTypes)
//					foreach (var right in numericTypes)
//					{
//						var rawEx = left[2] + " eq " + left[2] + " " + op + " " + right[2];
//						var exp = BuildLinqExpressionPredicate<Catalog2>(rawEx, _catalogRt);
//						var f = exp.Compile();
//					}
//		}
//
//		[Test]
//		public void NumericOps_On_NumericTypes_Literal()
//		{
//			foreach (var op in numericOps)
//				foreach (var left in numericTypes)
//					foreach (var right in numericTypes)
//					{
//						var rawEx = left[2] + " eq " + left[1] + " " + op + " " + right[1];
//						var exp = BuildLinqExpressionPredicate<Catalog2>(rawEx, _catalogRt);
//						var results = _catalogs.Where(exp.Compile());
//						results.Count();
//					}
//		}
//
//
//		public class Product2
//		{
//			[Key]
//			public int Id { get; set; }
//			public string Name { get; set; }
//		}
//
//		public class User2
//		{
//			[Key]
//			public int Id { get; set; }
//			public string Name { get; set; }
//			public string Email { get; set; }
//		}
//
//		public class Catalog2
//		{
//			[Key]
//			public int Id { get; set; }
//			public string Name { get; set; }
//			public IList<Product2> Products { get; set; }
//			public User2 Owner { get; set; }
//			public bool IsPublished { get; set; }
//			public int PropInt32 { get; set; }
//			public long PropInt64 { get; set; }
//			public decimal PropDec { get; set; }
//			public Single PropSingle { get; set; }
//			public double PropDouble { get; set; }
//		}
	}
}
