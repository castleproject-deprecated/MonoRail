namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class FilterParserTestCase
	{
		[Test]
		public void JustRunning()
		{
			// filter = Customers/ContactName ne 'Fred'
			var exp = FilterParser.parse("1 add 2 mul 3");
			Console.WriteLine(exp.ToStringTree());
		}
	}
}
