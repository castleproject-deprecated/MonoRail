using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerfComp1
{
    using System.Diagnostics;
    using System.Threading;
    using System.Web;
    using System.Web.Routing;
    using Castle.MonoRail.Routing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Route = System.Web.Routing.Route;

	class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

        	// MvcPerf();
			MRPerf();
        }

        static void MRPerf()
        {
            var router = new Router();
        	router.Match("(/:controller(/:action(/:id)))", new Mediator());

			var watch = new Stopwatch();
			watch.Start();

            // 1000000
			for (int i = 0; i < 1000000; i++)
            {
                // var context = new HttpContextWrapperStub("controller");
            	var req = new RequestInfoAdapter("/controller", null, null, null, null);
				var routeData = router.TryMatch(req);
                Assert.IsNotNull(routeData);

				req = new RequestInfoAdapter("/controller/action/10", null, null, null, null);
				routeData = router.TryMatch(req);
				Assert.IsNotNull(routeData);

				req = new RequestInfoAdapter("/controller/create", null, null, null, null);
				routeData = router.TryMatch(req); 
				Assert.IsNotNull(routeData);
            }
			watch.Stop();

			Console.WriteLine("MR+ Execution took " + watch.ElapsedMilliseconds);
        }

        static void MvcPerf()
        {
            var defaults = new RouteValueDictionary(new Dictionary<string, object>() 
                {{"controller", "home"}, {"action", "index"}, {"id", null}});
            RouteTable.Routes.Add("named", new Route("{controller}/{action}/{id}", defaults, new DummyHandler()));

            var collection = RouteTable.Routes;

            var watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < 1000000; i++)
            {
                var context = new HttpContextWrapperStub("controller");
                var routeData = collection.GetRouteData(context);
                Assert.IsNotNull(routeData);

                context = new HttpContextWrapperStub("controller/action/10");
                routeData = collection.GetRouteData(context);
                Assert.IsNotNull(routeData);

                context = new HttpContextWrapperStub("controller/create");
                routeData = collection.GetRouteData(context);
                Assert.IsNotNull(routeData);
            }

            watch.Stop();

            Console.WriteLine("MVC Execution took " + watch.ElapsedMilliseconds);
        }
    }

    internal class HttpContextWrapperStub : HttpContextBase
    {
        private HttpRequestBase _request;

        public HttpContextWrapperStub(string path)
        {
            _request = new HttpRequestStub("~/", path);
        }

        public override HttpRequestBase Request
        {
            get { return _request; }
        }
    }

    internal class HttpRequestStub : HttpRequestBase
    {
        // httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo
        private string _filePath, _pathInfo;

        public HttpRequestStub(string filePath, string pathInfo)
        {
            _filePath = filePath;
            _pathInfo = pathInfo;
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return _filePath; }
        }

        public override string PathInfo
        {
            get { return _pathInfo; }
        }
    }

    internal class DummyHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            throw new NotImplementedException();
        }
    }

	internal class Mediator : IRouteHttpHandlerMediator
	{
		public IHttpHandler GetHandler(HttpRequest request, RouteMatch routeData)
		{
			throw new NotImplementedException();
		}
	}
}
