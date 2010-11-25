namespace TestWebApp.Controller
{
	using System;
	using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
	using Castle.MonoRail.Primitives.Mvc;

	public class HomeController
	{
		private readonly ControllerContext _ctx;

		public HomeController(ControllerContext controllerContext)
		{
			_ctx = controllerContext;
		}

		public ActionResult Index()
		{
			// dynamic data = new PropertyBag();
			// data.Today = DateTime.Now;
			_ctx.Data["Today"] = DateTime.Now;

			return new ViewResult("index");
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
