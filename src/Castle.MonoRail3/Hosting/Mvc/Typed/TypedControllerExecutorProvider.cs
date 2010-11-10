namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail3.Primitives;

	[Export(typeof(ControllerExecutorProvider))]
	public class TypedControllerExecutorProvider : ControllerExecutorProvider
	{
		[Import]
		public ExportFactory<TypedControllerExecutor> ExecutorFactory { get; set; } 

		public override ControllerExecutor CreateExecutor(ControllerMeta meta, RouteData data, HttpContextBase context)
		{
			if (meta is TypedControllerMeta)
			{
				var executor = ExecutorFactory.CreateExport().Value;

				executor.Meta = meta as TypedControllerMeta;
				executor.RouteData = data;

				return executor;
			}

			return null;
		}
	}
}
