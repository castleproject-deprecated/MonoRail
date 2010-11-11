namespace Castle.MonoRail3.Hosting.Mvc
{
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;
	using Primitives;
	using Primitives.Mvc;

	[Export]
	public class PipelineRunner
	{
		[ImportMany]
		public IEnumerable<ControllerProvider> ControllerProviders { get; set; }

		[ImportMany]
		public IEnumerable<ControllerExecutorProvider> ControllerExecutorProviders { get; set; }

		public virtual void Process(RouteData data, HttpContextBase context)
		{
			ControllerMeta meta = InquiryProvidersForMetaController(data, context);

			if (meta == null)
				// how to improve the diagnostics story?
				throw new HttpException(404, "Not found");

			ControllerExecutor executor = GetExecutor(data, context, meta);

			if (executor == null)
				// need better exception model
				throw new HttpException(500, "Null executor ?!");

			executor.Process(context);
		}

		private ControllerExecutor GetExecutor(RouteData data, HttpContextBase context, ControllerMeta meta)
		{
			ControllerExecutor executor = null;

			foreach (var provider in ControllerExecutorProviders)
			{
				executor = provider.CreateExecutor(meta, data, context);
				if (executor != null) break;
			}

			return executor;
		}

		private ControllerMeta InquiryProvidersForMetaController(RouteData data, HttpContextBase context)
		{
			ControllerMeta meta = null;

			foreach (var provider in ControllerProviders)
			{
				meta = provider.Create(data, context);
				if (meta != null) break;
			}

			return meta;
		}
	}
}