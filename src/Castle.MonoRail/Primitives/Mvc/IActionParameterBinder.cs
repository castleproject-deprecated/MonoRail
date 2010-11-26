namespace Castle.MonoRail.Primitives.Mvc
{
	using System.Web;
	using MonoRail.Mvc.Typed;

	public interface IActionParameterBinder
	{
		object Bind(HttpContextBase httpContext, ParameterDescriptor descriptor);
	}
}
