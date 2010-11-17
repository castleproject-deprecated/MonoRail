namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System;
	using MonoRail3.Hosting.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class MethodInfoActionDescriptorTestCase
	{
		private string _a;
		private int _b;

		[Test]
		public void Should_build_reusable_lambda_function_for_method()
		{
			var descriptor = new MethodInfoActionDescriptor(GetType().GetMethod("LambdaTarget"));

			Assert.AreEqual("LambdaTarget", descriptor.Name);

			var lambdaContainer = new MethodInfoActionDescriptorTestCase();

			var date = (DateTime) descriptor.Action(lambdaContainer, new object[] { "string", 2 });

			Assert.AreEqual(DateTime.Now.Date, date);
			Assert.AreEqual("string", lambdaContainer._a);
			Assert.AreEqual(2, lambdaContainer._b);
		}

		public DateTime LambdaTarget(string a, int b)
		{
			_a = a;
			_b = b;

			return DateTime.Now.Date;
		}
	}
}
