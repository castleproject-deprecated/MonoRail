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