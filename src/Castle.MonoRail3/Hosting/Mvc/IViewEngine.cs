namespace Castle.MonoRail3.Hosting.Mvc
{
    using MonoRail.Hosting.Mvc;

    public interface IViewEngine
	{
		ViewEngineResult ResolveView(string viewName, string layout, ViewResolutionContext resolutionContext);

		void Release(IView view);
	}
}
