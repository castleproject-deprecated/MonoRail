namespace Castle.MonoRail.Hosting.Mvc
{
	using System.Collections.Generic;
	using MonoRail.Hosting.Mvc;

	public class ViewEngineResult
	{
		public ViewEngineResult(IEnumerable<string> searchedLocations)
		{
			SearchedLocations = searchedLocations;
		}

		public ViewEngineResult(IView view, IViewEngine viewEngine)
		{
			View = view;
			ViewEngine = viewEngine;
			Successful = true;
		}

		public bool Successful { get; private set; }
		public IEnumerable<string> SearchedLocations { get; set; }
		public IViewEngine ViewEngine { get; private set; }
		public IView View { get; private set; }
	}
}