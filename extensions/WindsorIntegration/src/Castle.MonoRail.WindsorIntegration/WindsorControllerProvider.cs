namespace Castle.MonoRail.WindsorIntegration
{
    using System.Collections;
    using System.ComponentModel.Composition;
    using System.Web;
    using System.Web.Routing;
    using Castle.MonoRail.Mvc;
    using Castle.MonoRail.Mvc.Rest;
    using Castle.MonoRail.Mvc.Typed;
    using Castle.MonoRail.Primitives.Mvc;
    using Castle.Windsor;

    [Export(typeof(ControllerProvider))]
    [ExportMetadata("Order", 1000)]
    // [PartCreationPolicy(CreationPolicy.Shared)]
    public class WindsorControllerProvider : ControllerProvider
    {
        private readonly HttpContextBase _httpContext;
        private readonly HttpRequestBase _request;
        private readonly HttpResponseBase _response;
        private readonly ContentNegotiator _contentNegotiator;
        private readonly ControllerContext _controllerContext;

        [ImportingConstructor]
        public WindsorControllerProvider(HttpContextBase httpContext, 
            HttpRequestBase request, HttpResponseBase response, 
            ContentNegotiator contentNegotiator, ControllerContext controllerContext)
        {
            _httpContext = httpContext;
            _request = request;
            _response = response;
            _contentNegotiator = contentNegotiator;
            _controllerContext = controllerContext;
        }

        [Import]
        public ControllerDescriptorBuilder DescriptorBuilder { get; set; }

        public override ControllerMeta Create(RouteData data)
        {
            var accessor = _httpContext.ApplicationInstance as IContainerAccessor;
            TypedControllerMeta meta = null;

            if (accessor != null)
            {
                var container = accessor.Container;
                var controllerName = data.GetRequiredString("controller").ToLowerInvariant();
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
            hashtable["httpContext"] = _httpContext;
            hashtable["request"] = _request;
            hashtable["response"] = _response;
            hashtable["contentNegotiator"] = _contentNegotiator;
            hashtable["controllerContext"] = _controllerContext;
            return hashtable;
        }
    }
}
