namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System.Web.Routing;
	using MonoRail.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives.Mvc;
	using Stubs;

	[TestFixture]
	public class ActionResultExecutionSinkTestCase
	{
		[Test]
		public void Invoke_should_execute_ActionResult_if_present()
		{
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			var sink = new ActionResultExecutionSink();
			var result = new TestActionResult();
			var context = new ControllerExecutionContext(null, this, new RouteData(), descriptor)
			              	{
			              		InvocationResult = result,
								SelectedAction = new TestActionDescriptor()
			              	};

			sink.Invoke(context);

			Assert.IsTrue(result.executed);
		}
	}
}
