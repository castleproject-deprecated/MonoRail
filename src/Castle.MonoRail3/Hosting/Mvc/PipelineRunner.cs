namespace Castle.MonoRail3.Hosting.Mvc
{
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;

	[Export]
	public class PipelineRunner
	{
		public virtual void Process(RouteData data, HttpContextBase context)
		{
		}
	}
}