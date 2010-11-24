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
	using System.Globalization;
	using System.Linq;
	using System.Web.Hosting;

    public abstract class VirtualPathProviderViewEngine : IViewEngine
	{
		private static readonly string[] EmptyLocations = new string[0];

		protected VirtualPathProviderViewEngine()
		{
			VirtualPathProvider = HostingEnvironment.VirtualPathProvider;
		}

		protected IEnumerable<string> AreaLayoutLocationFormats { get; set; }
		protected IEnumerable<string> AreaPartialViewLocationFormats { get; set; }
		protected IEnumerable<string> AreaViewLocationFormats { get; set; }
		protected IEnumerable<string> LayoutLocationFormats { get; set; }
		protected IEnumerable<string> PartialViewLocationFormats { get; set; }
		protected IEnumerable<string> ViewLocationFormats { get; set; }
		// Shouldn't this support composition? IOW be an import..
		protected VirtualPathProvider VirtualPathProvider { get; set; }

		public virtual ViewEngineResult ResolveView(string viewName, string layout, ViewResolutionContext resolutionContext)
		{
			string[] viewLocationsSearched;
			string[] masterLocationsSearched;

			string areaName = resolutionContext.AreaName;
			string controllerName = resolutionContext.ControllerName;
			string viewPath = GetPath(ViewLocationFormats, AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, areaName, out viewLocationsSearched);
			string layoutPath = GetPath(LayoutLocationFormats, AreaLayoutLocationFormats, "MasterLocationFormats", layout, controllerName, areaName, out masterLocationsSearched);

			if (String.IsNullOrEmpty(viewPath) ||
				(String.IsNullOrEmpty(layoutPath) && !String.IsNullOrEmpty(layout)))
			{
				return new ViewEngineResult(viewLocationsSearched.Union(masterLocationsSearched));
			}

			return new ViewEngineResult(CreateView(viewPath, layoutPath), this);
		}

		public void Release(IView view)
		{
			if (view is IDisposable)
			{
				try
				{
					(view as IDisposable).Dispose();
				}
				catch (Exception)
				{
					// swallow
				}
			}
		}

		protected abstract IView CreateView(string viewPath, string layoutPath);

		protected virtual bool FileExists(string path)
		{
			return VirtualPathProvider.FileExists(path);
		}

		private string GetPath(IEnumerable<string> locations, IEnumerable<string> areaLocations,
			string locationsPropertyName, string viewName, string controllerName, string areaName, out string[] searchedLocations)
		{
			searchedLocations = EmptyLocations;

			if (String.IsNullOrEmpty(viewName))
			{
				return String.Empty;
			}

			bool usingAreas = !String.IsNullOrEmpty(areaName);
			var viewLocations = GetViewLocations(locations, (usingAreas) ? areaLocations : null);

			if (viewLocations.Count == 0)
			{
				throw new InvalidOperationException(String.Format("Empty locations for property: {0}", locationsPropertyName));
			}

			bool nameRepresentsPath = IsSpecificPath(viewName);

			//            string cacheKey = CreateCacheKey(cacheKeyPrefix, viewName, (nameRepresentsPath) ? String.Empty : controllerName, areaName);
			//            if (useCache)
			//            {
			//                return ViewLocationCache.GetViewLocation(controllerContext.HttpContext, cacheKey);
			//            }

			return (nameRepresentsPath) ?
				GetPathFromSpecificName(viewName, ref searchedLocations) :
				GetPathFromGeneralName(viewLocations, viewName, controllerName, areaName, ref searchedLocations);
		}

		// changed to non-side-effecty
		private string GetPathFromGeneralName(IList<ViewLocation> locations, string name,
			string controllerName, string areaName, ref string[] searchedLocations)
		{
			string result = String.Empty;
			searchedLocations = new string[locations.Count];

			for (int i = 0; i < locations.Count; i++)
			{
				ViewLocation location = locations[i];
				string virtualPath = location.Format(name, controllerName, areaName);

				if (FileExists(virtualPath))
				{
					searchedLocations = EmptyLocations;
					result = virtualPath;
					// ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
					break;
				}

				searchedLocations[i] = virtualPath;
			}

			return result;
		}

		private string GetPathFromSpecificName(string name, ref string[] searchedLocations)
		{
			string result = name;

			if (!FileExists(name))
			{
				result = String.Empty;
				searchedLocations = new[] { name };
			}

			return result;
		}

		private static List<ViewLocation> GetViewLocations(IEnumerable<string> viewLocationFormats, IEnumerable<string> areaViewLocationFormats)
		{
			var allLocations = new List<ViewLocation>();

			if (areaViewLocationFormats != null)
			{
				foreach (string areaViewLocationFormat in areaViewLocationFormats)
				{
					allLocations.Add(new AreaAwareViewLocation(areaViewLocationFormat));
				}
			}

			if (viewLocationFormats != null)
			{
				foreach (string viewLocationFormat in viewLocationFormats)
				{
					allLocations.Add(new ViewLocation(viewLocationFormat));
				}
			}

			return allLocations;
		}

		private static bool IsSpecificPath(string name)
		{
			char c = name[0];
			return (c == '~' || c == '/');
		}

		private class ViewLocation
		{
			protected readonly string VirtualPathFormatString;

			public ViewLocation(string virtualPathFormatString)
			{
				VirtualPathFormatString = virtualPathFormatString;
			}

			public virtual string Format(string viewName, string controllerName, string areaName)
			{
				return String.Format(CultureInfo.InvariantCulture, VirtualPathFormatString, viewName, controllerName);
			}
		}

		private class AreaAwareViewLocation : ViewLocation
		{
			public AreaAwareViewLocation(string virtualPathFormatString)
				: base(virtualPathFormatString)
			{
			}

			public override string Format(string viewName, string controllerName, string areaName)
			{
				return String.Format(CultureInfo.InvariantCulture, VirtualPathFormatString, viewName, controllerName, areaName);
			}
		}
	}
}
