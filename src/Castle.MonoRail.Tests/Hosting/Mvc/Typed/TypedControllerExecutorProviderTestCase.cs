namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System;
	using System.ComponentModel.Composition;
	using System.Web.Routing;
	using MonoRail.Hosting.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class TypedControllerExecutorProviderTestCase
	{
		[Test]
		public void CreateExecutor_should_return_a_TypedControllerExecutor_ready_to_use()
		{
			var provider = new TypedControllerExecutorProvider {ExecutorFactory = new ExportFactory<TypedControllerExecutor>(GetExecutor)};
			var data = new RouteData();
			var meta = new TypedControllerMeta(null, null);

			var executor = (TypedControllerExecutor) provider.CreateExecutor(meta, data, null);

			Assert.AreSame(data, executor.RouteData);
			Assert.AreSame(meta, executor.Meta);
		}

		private static Tuple<TypedControllerExecutor, Action> GetExecutor()
		{
			return new Tuple<TypedControllerExecutor, Action>(new TypedControllerExecutor(null, null, null, null, null), () => {});
		}
	}
}
