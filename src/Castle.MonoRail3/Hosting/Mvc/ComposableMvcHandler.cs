namespace Castle.MonoRail3.Hosting.Mvc
{
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;

	[Export(typeof(IComposableHandler))]
	public class ComposableMvcHandler : ComposableHandler
	{
		[Import]
		public RequestParser RequestParser { get; set; }

		[Import]
		public PipelineRunner Runner { get; set; }

		// no state changes
		// what exceptions we should guard against?
		public override void ProcessRequest(HttpContextBase context)
		{
			RouteData data = RequestParser.ParseDescriminators(context.Request);

			Runner.Process(data, context);
		}
	}
}
