namespace Castle.MonoRail3.Hosting
{
	using System.Web;
	using Internal;

	public abstract class ComposableHandler : IHttpHandler, IComposableHandler
	{
		public abstract void ProcessRequest(HttpContextBase context);

		// non-disposables being added to container: fine no state changes
		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			var container = ContainerManager.CreateRequestContainer();
			
			container.HookOn(context);

			container.Compose(this);

			ProcessRequest(new HttpContextWrapper(context));
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}
