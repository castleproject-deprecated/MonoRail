namespace Castle.MonoRail.Extension.OData3.Tests.ExpressionParsing
{
	using System.Collections.Generic;
	using System.Linq;
	using FluentAssertions;
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Library;
	using MonoRail.OData.Internal;
	using NUnit.Framework;

	[TestFixture]
	public class OrderByParsingAndTranslationTestCase
	{
		[Test]
		public void orderby_with_single_Property()
		{
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");
			// var catEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var expressions = QueryExpressionParser.parse_orderby("Name");
			var orderByExps = QuerySemanticAnalysis.analyze_and_convert_orderby(expressions, new EdmEntityTypeReference(prodEdmType, false));

			orderByExps.Should().HaveCount(1);
			var order = orderByExps.ElementAt(0);
			order.IsAsc.Should().BeTrue();
			var orderByAsc = (OrderByAst.Asc) order;
			var prop = (QueryAst.PropertyAccess) orderByAsc.Item;
			prop.Item2.Name.Should().Be("Name");
		}

		[Test]
		public void orderby_with_single_Property_Explicit_DESC()
		{
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");
			// var catEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var expressions = QueryExpressionParser.parse_orderby("Name desc");
			var orderByExps = QuerySemanticAnalysis.analyze_and_convert_orderby(expressions, new EdmEntityTypeReference(prodEdmType, false));

			orderByExps.Should().HaveCount(1);
			var order = orderByExps.ElementAt(0);
			order.IsDesc.Should().BeTrue();
			var orderByAsc = (OrderByAst.Desc)order;
			var prop = (QueryAst.PropertyAccess)orderByAsc.Item;
			prop.Item2.Name.Should().Be("Name");
		}
	}
}
