namespace TestWebApp.Controller
{
	using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
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
	}
}