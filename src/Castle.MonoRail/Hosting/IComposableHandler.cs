namespace Castle.MonoRail.Hosting
{
	using System.Web;

	public interface IComposableHandler
	{
		void ProcessRequest(HttpContextBase context);
	}
}
