//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
namespace Castle.MonoRail.Mvc.ViewEngines
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using ViewEngines;

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
