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
namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Web.WebPages;
	using Hosting.Internal;
	using Primitives.Mvc;

	public class RazorView : IView
	{
		private readonly IHostingBridge hostingBridge;
		
		public RazorView(IHostingBridge hostingBridge, string view, string layout)
		{
			ViewPath = view;
			LayoutPath = layout;

			this.hostingBridge = hostingBridge;
		}

		private string LayoutPath { get; set; }

		private string ViewPath { get; set; }

		protected internal virtual object CreateViewInstance()
		{
			Type compiledType = hostingBridge.GetCompiledType(ViewPath);

			return Activator.CreateInstance(compiledType);
		}

		public void Process(ViewContext viewContext, TextWriter writer)
		{
			object view = CreateViewInstance();
			if (view == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"View could not be created : {0}", ViewPath));
			}

			WebViewPage initPage = view as WebViewPage;
			if (initPage == null)
			{
				throw new InvalidOperationException(string.Format(
					CultureInfo.CurrentCulture,
					"wrong base type for view: {0}", ViewPath));
			}

			//initPage.OverridenLayoutPath = this.LayoutPath;
			initPage.VirtualPath = this.ViewPath;
			initPage.Context = viewContext.HttpContext;
			initPage.Data = viewContext.Data;
			//initPage.InitHelpers();

			initPage.ExecutePageHierarchy(new WebPageContext(viewContext.HttpContext, initPage, null), writer, initPage);
		}
	}
}