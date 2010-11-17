namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed.Stubs
{
	using System;
	using Castle.MonoRail.Primitives.Mvc;

	public class TestActionDescriptor : ActionDescriptor
	{
		public TestActionDescriptor()
		{
			Name = "TestAction";
		}

		public TestActionDescriptor(Func<object, object[], object> theAction) : this()
		{
			Action = theAction;
		}
	}
}