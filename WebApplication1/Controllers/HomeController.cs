namespace WebApplication1.Controllers
{
	using Castle.MonoRail;

    public class Order
    {
    }

	public class HomeController
	{
		public ActionResult Index()
		{
//		    var order = new Order();
//		    var res = new ResourceResult<Order>(order);

            var res = new ViewResult() { LayoutName = "default" };
		    return res;
		}
	}
}