namespace Castle.MonoRail.ViewEngines.Razor
{
	using System.Web;

	public interface IViewPage
	{
		void SetData(object data);

		object GetData();

		string Layout { set; }

		string VirtualPath { set; }

		HttpContextBase Context { set; }
	}
}