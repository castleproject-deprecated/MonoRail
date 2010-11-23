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
#endregion

namespace Castle.MonoRail.Mvc.Typed
{
	using System.Web;
	using System.Web.Routing;

	public class ControllerExecutionContext
	{
		public ControllerExecutionContext(HttpContextBase httpContext, 
			object controller, RouteData data, 
			ControllerDescriptor controllerDescriptor)
		{
			HttpContext = httpContext;
			Controller = controller;
			RouteData = data;
			ControllerDescriptor = controllerDescriptor;
		}

		// readonly
		public object Controller { get; private set; }
		public HttpContextBase HttpContext { get; private set; }
		public RouteData RouteData { get; private set; }
		public ControllerDescriptor ControllerDescriptor { get; private set; }

		public object InvocationResult { get; set; }

		public ActionDescriptor SelectedAction { get; set; }
	}
}
