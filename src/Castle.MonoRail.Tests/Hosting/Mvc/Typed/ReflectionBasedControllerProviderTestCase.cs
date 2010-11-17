namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System.Web.Routing;
	using MonoRail3.Hosting.Internal;
	using MonoRail3.Hosting.Mvc.Typed;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ReflectionBasedControllerProviderTestCase
	{
		[Test]
		public void Create_should_create_requested_controller_instance()
		{
			var hosting = new Mock<IHostingBridge>();
			var data = new RouteData();
			data.Values.Add("controller", "sometest");

			hosting.SetupGet(bridge => bridge.ReferencedAssemblies).Returns(new[] {GetType().Assembly});

			var provider = new ReflectionBasedControllerProvider(hosting.Object)
			               	{
			               		DescriptorBuilder = new ControllerDescriptorBuilder()
			               	};

			var meta = provider.Create(data);

			Assert.IsNotNull(meta);
			Assert.IsInstanceOf<SomeTestController>(meta.ControllerInstance);
		}
	}

	public class SomeTestController
	{
		
	}
}
