namespace Castle.MonoRail3.Hosting.Internal
{
	using System;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.IO;
	using System.Web;

	static class ContainerManager
	{
		private const string RequestContainerKey = "infra.mr3.requestcontainer";

		private static readonly object locker = new object();

		private static ComposablePartCatalog catalog;
		private static ComposablePartCatalog requestCatalog;
		private static CompositionContainer container;

		// appdomain wide side effect (statics)
		static internal CompositionContainer GetOrCreateParentContainer()
		{
			if (container == null)
			{
				lock (locker)
				{
					if (container == null)
					{
						SubscribeToEndRequest();
						container = CreateContainer();
					}
				}
			}

			return container;
		}

		public static CompositionContainer GetRequestContainer()
		{
			var ctx = HttpContext.Current;

			var requestContainer = (CompositionContainer) ctx.Items[RequestContainerKey];

			if (requestContainer == null)
			{
				var parent = GetOrCreateParentContainer();
				requestContainer = new CompositionContainer(requestCatalog, parent);
				ctx.Items[RequestContainerKey] = requestContainer;
			}

			return requestContainer;
		}

		private static void SubscribeToEndRequest()
		{
			HttpContext.Current.ApplicationInstance.EndRequest += OnEndRequestDisposeContainer;
		}

		private static void OnEndRequestDisposeContainer(object sender, EventArgs e)
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
		private static CompositionContainer CreateContainer()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
			var binCatalog = new DirectoryCatalog(path);

			var partitioned = new PartitionedCatalog(binCatalog, p => !p.IsShared());
			requestCatalog = partitioned;
			ContainerManager.catalog = partitioned.Complement;

			return new CompositionContainer(ContainerManager.catalog); //TODO: needs to be made thread-safe
		}

		// Extension method to simplify filtering expression
		public static bool IsShared(this ComposablePartDefinition part)
		{
			var m = part.Metadata;
			object value;
			if (m.TryGetValue(CompositionConstants.PartCreationPolicyMetadataName, out value))
			{
				return ((CreationPolicy) value) == CreationPolicy.Shared;
			}
			return false;
		}
	}
}
