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
namespace TestWebApp.Controller
{
	using System;
	using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
	using Model;

	public class HomeController
	{
		public ViewResult Index(ControllerContext controllerContext)
		{
			dynamic data = controllerContext.Data;
			data.CreatedAt = DateTime.Now;

			return new ViewResult("index", "default");
		}

		public object Index2(ControllerContext controllerContext)
		{
			controllerContext.Data.MainModel = new Issue {CreatedAt = DateTime.Now};

			return new ViewResult("view");
		}

		public object About()
		{
			return new StringResult("Line Lanley");
		}
	}
}
