namespace Castle.MonoRail.Tests
{
	using System;
	using System.Web;
	using Hosting.Mvc.Typed;

	public class FakeActionDescriptor : ControllerActionDescriptor
	{
		public FakeActionDescriptor(string name)
			: base(name, new ControllerDescriptor(typeof(FakeController)))
		{
		}

		public override bool SatisfyRequest(HttpContextBase context)
		{
			return true;
		}

		public override object Execute(object instance, object[] args)
		{
			return instance;
		}

		public class FakeController
		{
			
		}
	}
}
