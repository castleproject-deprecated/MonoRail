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

namespace Castle.MonoRail.Internal
{
	using System;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.IO;
	using System.Web;

	public class ContainerManager
	{
		public const string RequestContainerKey = "infra.mr3.requestcontainer";

		private static readonly object locker = new object();

		private static CompositionContainer rootContainer;
		private static ComposablePartCatalog sharedCatalog;
		private static FilteredCatalog nonSharedCatalog;

		public static string CatalogPath { get; set; }

		private static void InitializeRootContainerIfNeeded()
		{
			if (rootContainer == null)
			{
				lock (locker)
				{
					if (rootContainer == null)
					{
						rootContainer = CreateContainer();
					}
				}
			}
		}

		public static CompositionContainer CreateRequestContainer(HttpContextBase ctx)
		{
			InitializeRootContainerIfNeeded();

			var requestContainer = new CompositionContainer(nonSharedCatalog, rootContainer);
			requestContainer.DisableSilentRejection = true;

			var batch = new CompositionBatch();
			batch.AddExportedValue(typeof(HttpRequestBase).GetContract(), ctx.Request);
			batch.AddExportedValue(typeof(HttpResponseBase).GetContract(), ctx.Response);
			batch.AddExportedValue(typeof(HttpContextBase).GetContract(), ctx);
			batch.AddExportedValue(typeof(HttpServerUtilityBase).GetContract(), ctx.Server);

			requestContainer.Compose(batch);

			return requestContainer;
		}

		public static void OnEndRequestDisposeContainer(object sender, EventArgs e)
		{
			var ctx = HttpContext.Current;

			var requestContainer = (CompositionContainer) ctx.Items[RequestContainerKey];

			if (requestContainer != null)
			{
				requestContainer.Dispose();
				ctx.Items[RequestContainerKey] = null;
			}
		}

		//TODO: catalog creation needs to be configurable
		public static CompositionContainer CreateContainer()
		{
			var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

			var directoryCatalog = new DirectoryCatalog(CatalogPath ?? defaultPath);
			var filteredCatalog = new FilteredCatalog(directoryCatalog, p => !p.IsShared());

			nonSharedCatalog = filteredCatalog;
			sharedCatalog = filteredCatalog.Complement;

			var container = new CompositionContainer(sharedCatalog, true);
			container.DisableSilentRejection = true;
			return container;
		}
	}
}
