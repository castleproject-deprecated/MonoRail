namespace TestWebApp.Controller
{
	using Castle.MonoRail;

	public class HomeController
	{
        public HomeController()
        {
            
        }

		public void Index()
		{
//			dynamic data = new PropertyBag();
//			data.Today = DateTime.Now;
//
//			return new ViewResult("index", "default", data);
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
