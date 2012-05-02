namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class QueryExpressionParserTestCase
	{
		[Test]
		public void JustRunning()
		{
			// filter = Customers/ContactName ne 'Fred'
			// var exp = QueryExpressionParser.parse("1 add 2 mul 3");
			var exp = QueryExpressionParser.parse("Customers/N ne 'Fred'");
			Console.WriteLine(exp.ToStringTree());
		}
	}
}
