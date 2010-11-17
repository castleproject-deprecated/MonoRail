namespace Castle.MonoRail3.ViewEngines.Razor
{
	using System.Web.WebPages;

	public abstract class WebViewPage : WebPageBase
	{
		public dynamic Data { get; set; }
	}
}