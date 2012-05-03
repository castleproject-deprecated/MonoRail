namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Data.Services.Providers;
	using NUnit.Framework;

	[TestFixture]
	public class QueryExpressionParserTestCase
	{
		[Test]
		public void JustRunning()
		{
			// filter = Customers/ContactName ne 'Fred'
			var exp0 = QueryExpressionParser.parse("1 add 2 mul 3");
			Console.WriteLine(exp0.ToStringTree());

			var exp1 = QueryExpressionParser.parse("Customers ne 'Fred'");
			Console.WriteLine(exp1.ToStringTree());

			var exp2 = QueryExpressionParser.parse("Customers/N ne 'Fred'");
			Console.WriteLine(exp2.ToStringTree());

			// QuerySemanticAnalysis.analyze(exp, null);
		}
	}
}
