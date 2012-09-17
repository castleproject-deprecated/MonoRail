namespace Castle.MonoRail.Extension.OData3.Tests.ExpressionParsing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using FluentAssertions;
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Library;
	using MonoRail.OData.Internal;
	using NUnit.Framework;

	[TestFixture]
	public class ExpandParsingAndTranslationTestCase
	{
		[Test, ExpectedException(typeof(Exception), ExpectedMessage = "Property not found: [Test]")]
		public void expand_with_nonexistent_Property()
		{
			var properties = new HashSet<IEdmProperty>();
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");

			var expressions = QueryExpressionParser.parse_expand("Test");
			QuerySemanticAnalysis.analyze_and_convert_expand(expressions, new EdmEntityTypeReference(prodEdmType, false), properties);
		}

		[Test]
		public void expand_with_single_Property()
		{
			var properties = new HashSet<IEdmProperty>();
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");
			// var catEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var expressions = QueryExpressionParser.parse_expand("Categories");
			QuerySemanticAnalysis.analyze_and_convert_expand(expressions, new EdmEntityTypeReference(prodEdmType, false), properties);

			properties.Should().HaveCount(1);
			properties.ElementAt(0).Name.Should().Be("Categories");
		}

		[Test]
		public void expand_with_two_Properties()
		{
			var properties = new HashSet<IEdmProperty>();
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");
			// var catEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var expressions = QueryExpressionParser.parse_expand("Categories,Name");
			QuerySemanticAnalysis.analyze_and_convert_expand(expressions, new EdmEntityTypeReference(prodEdmType, false), properties);

			properties.Should().HaveCount(2);
			properties.ElementAt(0).Name.Should().Be("Categories");
			properties.ElementAt(1).Name.Should().Be("Name");
		}

		[Test]
		public void expand_with_path_property()
		{
			var properties = new HashSet<IEdmProperty>();
			var model = Models.ModelWithIndirection.Build();
			var prodEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Product");
			var catEdmType = (IEdmEntityType)model.FindDeclaredType("schema.Category");

			var expressions = QueryExpressionParser.parse_expand("Categories/Name");
			QuerySemanticAnalysis.analyze_and_convert_expand(expressions, new EdmEntityTypeReference(prodEdmType, false), properties);

			properties.Should().HaveCount(1);
			properties.ElementAt(0).DeclaringType.Should().Be(catEdmType);
			properties.ElementAt(0).Name.Should().Be("Name");
		}
	}
}
