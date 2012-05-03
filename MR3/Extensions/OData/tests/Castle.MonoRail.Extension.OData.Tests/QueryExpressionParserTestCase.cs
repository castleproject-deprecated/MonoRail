namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.Linq;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class QueryExpressionParserTestCase
	{
		[Test]
		public void LogicalOp_Ne()
		{
			var exp = QueryExpressionParser.parse("City ne 'Redmond'");
			exp.ToStringTree().Should().BeEquivalentTo(@"
  Binary Neq 
    MemberAccess 
      Element
      Id City
    Literal SString [Redmond]");
		}

		[Test]
		public void LogicalOp_Eq()
		{
			var exp = QueryExpressionParser.parse("City eq 'Redmond'");
			// Console.WriteLine(exp.ToStringTree());
			exp.ToStringTree().Should().BeEquivalentTo(@"
  Binary Eq 
    MemberAccess 
      Element
      Id City
    Literal SString [Redmond]");
		}

		[Test]
		public void LogicalOp_GreaterThan()
		{
			var exp = QueryExpressionParser.parse("Price gt 10");
			Console.WriteLine(exp.ToStringTree());
			
		}

		[Test]
		public void LogicalOp_GreaterThanOrEqual()
		{
			var exp = QueryExpressionParser.parse("Price ge 10");
			Console.WriteLine(exp.ToStringTree());

		}

		[Test]
		public void LogicalOp_LessThan()
		{
			var exp = QueryExpressionParser.parse("Price lt 10");
			Console.WriteLine(exp.ToStringTree());

		}

		[Test]
		public void LogicalOp_LessThanOrEqual()
		{
			var exp = QueryExpressionParser.parse("Price le 10");
			Console.WriteLine(exp.ToStringTree());

		}

		[Test]
		public void LogicalOp_And()
		{
			var exp = QueryExpressionParser.parse("Price le 200 and Price gt 3.5");
			Console.WriteLine(exp.ToStringTree());

		}

		[Test]
		public void LogicalOp_Or()
		{
			var exp = QueryExpressionParser.parse("Price le 3.5 or Price gt 200");
			Console.WriteLine(exp.ToStringTree());
		}

		[Test]
		public void LogicalOp_Not()
		{
			var exp = QueryExpressionParser.parse("not endswith(Description,'milk')");
			Console.WriteLine(exp.ToStringTree());
		}

		[Test]
		public void Arithmetic_Add()
		{
			var exp = QueryExpressionParser.parse("Price add 5 gt 10");
			Console.WriteLine(exp.ToStringTree());
		}
		[Test]
		public void Arithmetic_Sub()
		{
			var exp = QueryExpressionParser.parse("Price sub 5 gt 10");
			Console.WriteLine(exp.ToStringTree());
		}
		[Test]
		public void Arithmetic_Mul()
		{
			var exp = QueryExpressionParser.parse("Price mul 2 gt 2000");
			Console.WriteLine(exp.ToStringTree());
		}

		[Test]
		public void Arithmetic_Div()
		{
			var exp = QueryExpressionParser.parse("Price div 2 gt 4");
			Console.WriteLine(exp.ToStringTree());
		}
		[Test]
		public void Arithmetic_Mod()
		{
			var exp = QueryExpressionParser.parse("Price mod 2 eq 0");
			Console.WriteLine(exp.ToStringTree());
		}


		[Test]
		public void OperatorPrecedence()
		{
			var exp0 = QueryExpressionParser.parse("1 add 2 mul 3");
			Console.WriteLine(exp0.ToStringTree());

			exp0 = QueryExpressionParser.parse("1 mul 2 add 3");
			Console.WriteLine(exp0.ToStringTree());

			exp0 = QueryExpressionParser.parse("1 add 2 add 3");
			Console.WriteLine(exp0.ToStringTree());

			// Equivalent
			exp0 = QueryExpressionParser.parse("1 sub 2 add 3");
			Console.WriteLine(exp0.ToStringTree());

			// parens
			exp0 = QueryExpressionParser.parse("1 mul (2 add 3)");
			Console.WriteLine(exp0.ToStringTree());
		}


		[Test]
		public void PropNavigation()
		{
			var exp1 = QueryExpressionParser.parse("Name ne 'Cat1'");
			Console.WriteLine(exp1.ToStringTree());

			var exp2 = QueryExpressionParser.parse("Customers/Name eq 'John'");
			Console.WriteLine(exp2.ToStringTree());

			var exp3 = QueryExpressionParser.parse("Customers/Address/Street eq 'London Ave'");
			Console.WriteLine(exp3.ToStringTree());
		}



	}
}
