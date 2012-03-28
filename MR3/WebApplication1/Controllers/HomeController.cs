namespace WebApplication1.Controllers
{
	using System;
	using Castle.MonoRail;

	public partial class HomeController
	{
		public ActionResult Index()
		{
            var res = new ViewResult(); //{ LayoutName = "default" };
			throw new Exception("Hey you");
		    //return res;
		}

		public ActionResult Components()
		{
			return new ViewResult();
		}

//        public ActionResult Create(Model<Order> order)
//        {
//            
//
//            return new RedirectResult();
//        }
	}
}