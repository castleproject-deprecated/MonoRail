namespace TestWebApp.Controller
{
    using System;
    using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
    using Castle.MonoRail.Primitives.Mvc;

    public class HomeController
	{
        private readonly ControllerContext _ctx;

        public HomeController(ControllerContext ctx)
        {
            _ctx = ctx;
        }

        public ActionResult Index()
		{
			dynamic data = new PropertyBag();
			data.Today = DateTime.Now;
            _ctx.Data = data;

			return new ViewResult("index", "default");
		}

		public object Index2()
		{
			return new ViewResult("view");
		}

		public object About()
		{
			return new StringResult("Line Lanley");
		}
	}
}
