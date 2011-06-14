namespace WebApplication1.Controllers.ViewComponents.OrdersComponent
{
	using Castle.MonoRail;
	using Castle.MonoRail.Helpers;
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