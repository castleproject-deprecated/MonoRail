namespace Castle.MonoRail3.Hosting
{
	using System.Web;

	public interface IComposableHandler
	{
		void ProcessRequest(HttpContextBase context);
	}
}
