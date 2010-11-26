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
namespace Castle.MonoRail.ViewEngines.Razor
{
	using System.ComponentModel.Composition;
	using System.Web.Compilation;
	using System.Web.WebPages.Razor;
	using Internal;
	using Mvc.ViewEngines;

	[Export(typeof(IViewEngine))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class RazorViewEngine : VirtualPathProviderViewEngine
	{
		[Import]
		public IHostingBridge HostingBridge { get; set; }

		public RazorViewEngine()
		{
			AreaViewLocationFormats = new[] {
				"~/Areas/{2}/Views/{1}/{0}.cshtml", 
				"~/Areas/{2}/Views/Shared/{0}.cshtml"
			};
			AreaLayoutLocationFormats = new[] {
				"~/Areas/{2}/Views/{1}/{0}.cshtml", 
				"~/Areas/{2}/Views/Shared/{0}.cshtml"
			};
			AreaPartialViewLocationFormats = new[] {
				"~/Areas/{2}/Views/{1}/{0}.cshtml", 
				"~/Areas/{2}/Views/Shared/{0}.cshtml"
			};
			ViewLocationFormats = new[] {
				"~/Views/{1}/{0}.cshtml", 
				"~/Views/Shared/{0}.cshtml"
			};
			LayoutLocationFormats = new[] {
				"~/Views/{1}/{0}.cshtml", 
				"~/Views/Shared/{0}.cshtml"
			};
			PartialViewLocationFormats = new[] {
				"~/Views/{1}/{0}.cshtml", 
				"~/Views/Shared/{0}.cshtml"
			};
		}

		protected override IView CreateView(string viewPath, string layoutPath)
		{
			return new RazorView(HostingBridge, viewPath, layoutPath);
		}

		protected override bool FileExists(string path)
		{
			return HostingBridge.FileExists(path);
		}

		public static void Initialize()
		{
			BuildProvider.RegisterBuildProvider(".cshtml", typeof(RazorBuildProvider));
		}
	}
}
