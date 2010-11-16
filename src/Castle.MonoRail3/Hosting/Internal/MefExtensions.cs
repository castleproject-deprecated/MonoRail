using System.Threading;

namespace Castle.MonoRail3.Hosting.Internal
{
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.Web;

	public static class MefExtensions
	{
		private static bool hookedEndRequest = false;
	    private static object staticLocker = new object();

		// Extension method to simplify filtering expression
		public static bool IsShared(this ComposablePartDefinition part)
		{
			object value;

			if (part.Metadata.TryGetValue(CompositionConstants.PartCreationPolicyMetadataName, out value))
			{
				return ((CreationPolicy) value) == CreationPolicy.Shared;
			}

			return false;
		}

		public static CompositionBatch Compose(this CompositionContainer container, IComposableHandler handler)
		{
			var batch = new CompositionBatch();

			batch.AddPart(handler);

			container.Compose(batch);

			return batch;
		}

		public static CompositionContainer HookOn(this CompositionContainer container, HttpContext httpContext)
		{
			httpContext.Items[ContainerManager.RequestContainerKey] = container;

            // Avoid race condition
			if (!hookedEndRequest)
			{
                lock (staticLocker)
                {
                    if (!hookedEndRequest)
                    {
                        httpContext.ApplicationInstance.EndRequest += ContainerManager.OnEndRequestDisposeContainer;

                        Thread.MemoryBarrier();
                        hookedEndRequest = true;
                    }
                }

			}

			return container;
		}
	}
}