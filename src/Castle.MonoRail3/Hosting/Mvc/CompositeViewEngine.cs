namespace Castle.MonoRail3.Hosting.Mvc
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using MonoRail.Hosting.Mvc;

	// note that it DOES NOT export IViewEngine
    [Export] 
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CompositeViewEngine : IViewEngine
    {
        [ImportMany]
        public IEnumerable<IViewEngine> ViewEngines { get; set; }

        public ViewEngineResult ResolveView(string viewName, string layout, ViewResolutionContext resolutionContext)
        {
            var failedResults = new List<ViewEngineResult>();

            foreach(var viewEngine in ViewEngines)
            {
                var result = viewEngine.ResolveView(viewName, layout, resolutionContext);
                
                if (result.Successful)
                    return result;
                else
                    failedResults.Add(result);
            }

            return new ViewEngineResult(failedResults.SelectMany(res => res.SearchedLocations));
        }

        public void Release(IView view)
        {
            // should not be invoked on the composite
            throw new NotImplementedException();
        }
    }
}
