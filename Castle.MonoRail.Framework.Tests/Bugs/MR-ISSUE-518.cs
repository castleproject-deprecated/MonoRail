namespace Castle.MonoRail.Framework.Tests.Bugs
{
	using System.Collections.Specialized;
	using NUnit.Framework;
	using Test;

	[TestFixture]
	public class MR_ISSUE_518
	{
		StubRequest request;
		StubResponse response;
		StubMonoRailServices services;
		StubEngineContext engineContext;

		[SetUp]
		public void Init()
		{
			request = new StubRequest();
			response = new StubResponse();
			services = new StubMonoRailServices();
			services = new StubMonoRailServices { ViewEngineManager = new StubViewEngineManager() };
			engineContext = new StubEngineContext(request, response, services, null);
		}

		[Test]
		public void Controller_RedirectToAction_With_Specified_Anchor()
		{
			services.UrlBuilder.UseExtensions = false;

			var controller = new ControllerWithRedirect();

			var context = services.ControllerContextFactory.
				Create("", "home", "RedirectToActionWithAnchor", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			engineContext.CurrentController = controller;
			engineContext.CurrentControllerContext = context;

			controller.Process(engineContext, context);

			Assert.IsTrue(response.WasRedirected);
			Assert.That(response.RedirectedTo, Is.EqualTo("/home/action?id=1#tab1"));
		}

		[Test]
		public void Controller_RedirectToAction_With_Specified_Anchor_Using_NameValueCollection()
		{
			services.UrlBuilder.UseExtensions = false;

			var controller = new ControllerWithRedirect();

			var context = services.ControllerContextFactory.
				Create("", "home", "RedirectToActionWithAnchorPassingNameValueCollection", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			engineContext.CurrentController = controller;
			engineContext.CurrentControllerContext = context;

			controller.Process(engineContext, context);

			Assert.IsTrue(response.WasRedirected);
			Assert.That(response.RedirectedTo, Is.EqualTo("/home/action?id=1#tab1"));
		}

		class ControllerWithRedirect : Controller
		{
			public void RedirectToActionWithAnchor()
			{
				RedirectToAction("action", "id=1", "#tab1");
			}

			public void RedirectToActionWithAnchorPassingNameValueCollection()
			{
				var urlParams = new NameValueCollection {{"id", "1"}, {"#tab1", ""}};
				RedirectToAction("action", urlParams);
			}
		}
	}
}