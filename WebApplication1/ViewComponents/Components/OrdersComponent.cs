namespace WebApplication1.ViewComponents.Components
{
	using Castle.MonoRail;
	using Castle.MonoRail.Hosting.Mvc.Typed;

	public partial class OrdersComponent : IViewComponent
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