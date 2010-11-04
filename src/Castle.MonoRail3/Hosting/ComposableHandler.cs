namespace Castle.MonoRail3.Hosting
{
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition;
	using System.Web;
	using Internal;

	public abstract class ComposableHandler : IHttpHandler, IComposableHandler
	{
		public abstract void ProcessRequest(HttpContextBase context);

		// non-disposables being added to container: fine no state changes
		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			var batch = new CompositionBatch();
			batch.AddPart(this);

			ContainerManager.GetRequestContainer().Compose(batch);

			ProcessRequest(new HttpContextWrapper(context));
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}
