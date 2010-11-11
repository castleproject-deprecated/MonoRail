namespace Castle.MonoRail3.Tests.Hosting.Mvc.Typed
{
	using System;
	using System.Web.Routing;
	using MonoRail3.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives.Mvc;

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

	public class TestActionResult : ActionResult
	{
		public bool executed;

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			executed = true;
		}
	}
}
