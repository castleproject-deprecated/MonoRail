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
namespace Castle.MonoRail.Mvc.ViewEngines
{
	using System.ComponentModel.Composition;
	using Internal;
	using WebForms;

	[Export(typeof(IViewEngine))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class WebFormsViewEngine : VirtualPathProviderViewEngine
	{
		[Import]
		public IHostingBridge HostingBridge { get; set; }

		[Import]
		public IWebFormFactory WebFormFactory { get; set; }

		public WebFormsViewEngine()
		{
			LayoutLocationFormats = new[] {
				"~/Views/{1}/{0}.master",
				"~/Views/Shared/{0}.master"
			};

			AreaLayoutLocationFormats = new[] {
				"~/Areas/{2}/Views/{1}/{0}.master",
				"~/Areas/{2}/Views/Shared/{0}.master",
			};

			ViewLocationFormats = new[] {
				"~/Views/{1}/{0}.aspx",
				"~/Views/{1}/{0}.ascx",
				"~/Views/Shared/{0}.aspx",
				"~/Views/Shared/{0}.ascx"
			};

			AreaViewLocationFormats = new[] {
				"~/Areas/{2}/Views/{1}/{0}.aspx",
				"~/Areas/{2}/Views/{1}/{0}.ascx",
				"~/Areas/{2}/Views/Shared/{0}.aspx",
				"~/Areas/{2}/Views/Shared/{0}.ascx",
			};

			PartialViewLocationFormats = ViewLocationFormats;
			AreaPartialViewLocationFormats = AreaViewLocationFormats;
		}

		protected override IView CreateView(string viewPath, string layoutPath)
		{
			return new WebFormView(this.WebFormFactory, viewPath, layoutPath);
		}

		protected override bool FileExists(string path)
		{
			return HostingBridge.FileExists(path);
		}
	}

}
