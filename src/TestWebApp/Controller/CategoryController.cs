namespace TestWebApp.Controller
{
	using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
	using Castle.MonoRail.Mvc.Typed;
	using Model;

	public class CategoryController
	{
		private readonly ControllerContext controllerContext;

		public CategoryController(ControllerContext controllerContext)
		{
			this.controllerContext = controllerContext;
		}

		public object Index()
		{
			var categories = new[] {new Category("Bugs"), new Category("Improvement")};

			controllerContext.Data["Categories"] = categories;

			return new ViewResult();
		}

		public ViewResult Save([DataBind] Category category)
		{
			controllerContext.Data["Categories"] = new Category[0];

			controllerContext.Data["category"] = category;

			return new ViewResult("index");
		}
	}
}