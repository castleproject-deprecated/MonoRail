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
	public class FilterParsingAndTranslationTestCase
	{
		[Test]
		public void simple_binary_expression()
		{
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var expression = QueryExpressionParser.parse_filter("Id eq 1");
			var ast = QuerySemanticAnalysis.analyze_and_convert(expression, prodEdmType);

			ast.Should().NotBeNull();
			ast.ToStringTree().Should().Be(
@"
  Binary Eq Boolean
    PropertyAccess [Id] = Int32
      Element
    Literal Int32 [1]");
		}
	}
}
