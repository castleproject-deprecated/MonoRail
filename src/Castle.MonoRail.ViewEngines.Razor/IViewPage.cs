namespace Castle.MonoRail.ViewEngines.Razor
{
	using System.Web;
	using Primitives.Mvc;

	public interface IViewPage
	{
		void SetData(object model);

		object GetData();

		string Layout { set; }

		string VirtualPath { set; }

		HttpContextBase Context { set; }

		DataContainer DataContainer { get; set; }
	}
}