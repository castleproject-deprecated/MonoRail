#region License
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
#endregion
namespace Castle.MonoRail
{
	using System;
	using Mvc;
	using Mvc.Typed;
	using Mvc.ViewEngines;

//    public class ViewResult<T> : ViewResult
//    {
//        
//    }

	public class ViewResult : ActionResult
	{
		public string ViewName { get; set; }
		public string Layout { get; set; }

		public ViewResult()
		{
		}

		public ViewResult(string viewName, string layout = null)
		{
			ViewName = viewName;
			Layout = layout;
		}

		public override void Execute(ActionResultContext context, ControllerContext controllerContext, IMonoRailServices services)
		{
			ApplyConventions(context);

			var viewEngines = services.ViewEngines;
			
			var result = viewEngines.ResolveView(this.ViewName, this.Layout, new ViewResolutionContext(context));

			if (result.Successful)
			{
				try
				{
					var httpContext = context.HttpContext;
					var viewContext = new ViewContext(httpContext, httpContext.Response.Output, controllerContext);

					result.View.Process(viewContext, httpContext.Response.Output);
				}
				finally
				{
					result.ViewEngine.Release(result.View);
				}
			}
			else
			{
				throw new Exception("Could not find view " + this.ViewName +
					". Searched at " + string.Join(", ", result.SearchedLocations));
			}
		}

        private void ApplyConventions(ActionResultContext context)
        {
            if (this.ViewName == null)
            {
                this.ViewName = context.ActionName;
            }
        }
	}
}
