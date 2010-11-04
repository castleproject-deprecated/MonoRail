namespace Castle.MonoRail3.Hosting.Internal
{
	using System;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.IO;
	using System.Web;

	public class ContainerManager
	{
		internal const string RequestContainerKey = "infra.mr3.requestcontainer";

		private static readonly object locker = new object();

		private static CompositionContainer rootContainer;
		private static ComposablePartCatalog sharedCatalog;
		private static PartitionedCatalog nonSharedCatalog;

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

		public static CompositionContainer CreateRequestContainer()
		{
			InitializeRootContainerIfNeeded();

			var requestContainer = new CompositionContainer(nonSharedCatalog, rootContainer);

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

			var partitioned = new PartitionedCatalog(directoryCatalog, p => !p.IsShared());

			nonSharedCatalog = partitioned;
			sharedCatalog = partitioned.Complement;

			return new CompositionContainer(sharedCatalog); //TODO: needs to be made thread-safe
		}
	}
}
