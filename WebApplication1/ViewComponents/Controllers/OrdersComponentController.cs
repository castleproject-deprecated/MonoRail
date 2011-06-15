namespace WebApplication1.ViewComponents.Controllers
{
	using Castle.MonoRail;
	using Castle.MonoRail.Hosting.Mvc.Typed;

	public class OrdersComponentController : IViewComponent
	{
		public string SomeCriteria { get; set; }

		public ActionResult Refresh()
		{
			return new ViewResult{ViewName = "default"};
		}

		public ViewComponentResult Render()
		{
			return new ViewComponentResult();
		}
	}
}