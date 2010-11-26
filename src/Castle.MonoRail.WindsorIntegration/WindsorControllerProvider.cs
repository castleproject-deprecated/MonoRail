namespace Castle.MonoRail.WindsorIntegration
{
	using System.Collections;
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail.Mvc;
	using Castle.MonoRail.Mvc.Typed;
	using Castle.MonoRail.Primitives.Mvc;
	using Castle.Windsor;

	[Export(typeof(ControllerProvider))]
	[ExportMetadata("Order", 1000)]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class WindsorControllerProvider : ControllerProvider
	{
		private readonly HttpContextBase httpContext;
		private readonly HttpRequestBase request;
		private readonly HttpResponseBase response;
		private readonly ControllerContext controllerContext;

		[ImportingConstructor]
		public WindsorControllerProvider(HttpContextBase httpContext, HttpRequestBase request, HttpResponseBase response, ControllerContext controllerContext)
		{
			this.httpContext = httpContext;
			this.request = request;
			this.response = response;
			this.controllerContext = controllerContext;
		}

		[Import]
		public ControllerDescriptorBuilder DescriptorBuilder { get; set; }

		public override ControllerMeta Create(RouteData data)
		{
			TypedControllerMeta meta = null;

			var accessor = httpContext.ApplicationInstance as IContainerAccessor;
			if (accessor != null)
			{
				var container = accessor.Container;
				var controllerName = data.GetRequiredString("controller").ToLowerInvariant() + "controller";

				if (!container.Kernel.HasComponent(controllerName)) return null;

				var args = CreateArgs();
				var controller = container.Resolve<object>(controllerName, args);

				var descriptor = DescriptorBuilder.Build(controller.GetType());

				meta = new TypedControllerMeta(controller, descriptor);
			}

			return meta;
		}

		private IDictionary CreateArgs()
		{
			var hashtable = new Hashtable();
			hashtable["httpContext"] = httpContext;
			hashtable["request"] = request;
			hashtable["response"] = response;
			hashtable["controllerContext"] = controllerContext;
			return hashtable;
		}
	}
}