namespace Castle.MonoRail.Mvc
{
	using System.Web;
	using Castle.MonoRail.Mvc.Typed;

	public interface IActionParameterBinder
	{
		object Bind(HttpContextBase httpContext, ParameterDescriptor descriptor);
	}
}
