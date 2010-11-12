namespace TestWebApp.Controller
{
	using Castle.MonoRail3;

	public class HomeController
	{
		public object Index()
		{
			return new StringResult("Line Lanley");
		}
	}
}