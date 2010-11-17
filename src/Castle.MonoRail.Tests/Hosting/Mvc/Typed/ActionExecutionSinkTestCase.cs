namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System.Web.Routing;
	using MonoRail3.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives.Mvc;
	using Stubs;

	[TestFixture]
	public class ActionExecutionSinkTestCase
	{
		private bool invoked;

		[Test]
		public void Invoke_should_execute_the_selected_action()
		{
			var sink = new ActionExecutionSink();

			var context = new ControllerExecutionContext(null, this, new RouteData(), null)
			              	{
			              		SelectedAction = new TestActionDescriptor(TheAction)
			              	};

			sink.Invoke(context);

			Assert.IsTrue(invoked);
		}

		public object TheAction(object target, object[] args)
		{
			invoked = true;

			return new object();
		}
	}
}
