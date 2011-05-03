namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using Castle.MonoRail;
	using Castle.MonoRail.Routing;

	public partial class HomeController
	{
        public abstract class Urls
        {
            private static TargetUrl _field1;

            public static TargetUrl Index()
            {
                return _field1;
            }
        }
	}

    public partial class TodoController
    {
        public abstract class Urls
        {
            private static Router _router;
//            private static TargetUrl _field1;
//            private static TargetUrl _field2;
//            private static TargetUrl _field3;
//            private static TargetUrl _field4;
//            private static TargetUrl _field5;
//            private static TargetUrl _field6;

            private static Router Current
            {
                get 
                { 
                    if (_router == null)
                        _router = Router.Instance;
                    return _router;
                }
                set { _router = value; }
            }

//            public static TargetUrl Index()
//            {
//                if (_field1 == null)
//                {
                    // _field1 = // new TargetUrl(Current.Routes["default"]);
//                }
//                return _field1;
//            }
//
//            public static TargetUrl View(int id)
//            {
//                return _field2;
//            }
//
//            public static TargetUrl Edit(int id)
//            {
//                return _field3;
//            }

            public static TargetUrl Create()
            {
                return new RoutedTargetUrl();
            }

//            public static TargetUrl Update()
//            {
//                return _field5;
//            }
//
//            public static TargetUrl Delete(int id)
//            {
//                return _field6;
//            }
        }
    }


    class RoutedTargetUrl : TargetUrl
    {
        public override void Generate(IDictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }

//    public class TargetUrl
//    {
//        private readonly Route _route;
//
//        public TargetUrl(Route route)
//        {
//            _route = route;
//        }
//
//        public string GetUrl()
//        {
//            return _route.Generate();
//        }
//    }
}