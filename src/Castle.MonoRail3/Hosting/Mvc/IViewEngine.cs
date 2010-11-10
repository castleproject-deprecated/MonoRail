namespace Castle.MonoRail3.Hosting.Mvc
{
	using Primitives.Mvc;
	using MonoRail.Hosting.Mvc;

	public class ViewResolutionContext : BaseMvcContext
	{
		public ViewResolutionContext(BaseMvcContext copy) : base(copy)
		{
		}

		public ViewResolutionContext(string areaName, string controllerName, string actionName) : 
			base(areaName, controllerName, actionName)
		{
		}
	}

	public interface IViewEngine
	{
		ViewEngineResult ResolveView(string viewName, string layout, ViewResolutionContext resolutionContext);

		void Release(IView view);
	}
}
