namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System.Web;
	using System.Web.Routing;
	using MonoRail.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives.Mvc;
	using Stubs;

	[TestFixture]
	public class ActionResolutionSinkTestCase
	{
		[Test]
		public void Invoke_should_find_action_on_controller()
		{
			var data = new RouteData();
			data.Values.Add("action", "TestAction");

			var sink = new ActionResolutionSink();
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			descriptor.Actions.Add(new TestActionDescriptor());

			var context = new ControllerExecutionContext(null, this, data, descriptor);

			sink.Invoke(context);

			Assert.IsNotNull(context.SelectedAction);
			Assert.AreEqual("TestAction", context.SelectedAction.Name);
		}

		[Test, ExpectedException(typeof(HttpException))]
		public void Invoke_should_thrown_an_404_if_cannot_find_a_action()
		{
			var data = new RouteData();
			data.Values.Add("action", "Foo");

			var sink = new ActionResolutionSink();
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			descriptor.Actions.Add(new TestActionDescriptor());

			var context = new ControllerExecutionContext(null, this, data, descriptor);

			sink.Invoke(context);
		}
	}
}
