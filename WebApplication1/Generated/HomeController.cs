namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Castle.MonoRail;
	using Castle.MonoRail.Routing;

    public partial class HomeController
    {
        public abstract class Urls
        {
            private static string _vpath;
            private static Router _router;

            internal static string VirtualPath
            {
                get
                {
                    if (_vpath == null)
                        _vpath = HttpContext.Current.Request.ApplicationPath;
                    return _vpath;
                }
                set
                {
                    _vpath = value;
                }
            }

            internal static Router Current
            {
                get
                {
                    if (_router == null)
                        _router = Router.Instance;
                    return _router;
                }
                set { _router = value; }
            }

            public static TargetUrl Index()
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "home" }, { "action", "index" } });
            }
        }
    }

    public partial class TodoController
    {
        public abstract class Urls
        {
            private static string _vpath;
            private static Router _router;

            internal static string VirtualPath
            {
                get
                {
                    if (_vpath == null)
                        _vpath = HttpContext.Current.Request.ApplicationPath;
                    return _vpath;
                }
                set
                {
                    _vpath = value;
                }
            }

            internal static Router Current
            {
                get
                {
                    if (_router == null)
                        _router = Router.Instance;
                    return _router;
                }
                set { _router = value; }
            }

            public static TargetUrl Index()
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "index" } });
            }

            public static TargetUrl View(int id)
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" }, {"view", id.ToString()} });
            }

            public static TargetUrl New()
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" } });
            }

            public static TargetUrl Edit(int id)
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" }, { "view", id.ToString() } });
            }

            public static TargetUrl Create()
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" } });
            }

            public static TargetUrl Update()
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" } });
            }

            public static TargetUrl Delete(int id)
            {
                return new RouteBasedTargetUrl(VirtualPath, Current.Routes["default"],
                    new Dictionary<string, string>() { { "controller", "todo" }, { "action", "view" }, { "view", id.ToString() } });
            }
        }
    }
}