namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.Web;
	using Components.Binder;

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple=false)]
	public class DataBindAttribute : Attribute, IActionParameterBinder
	{
		public DataBindAttribute()
		{
		}

		public DataBindAttribute(string prefix)
		{
			Prefix = prefix;
		}

		public string Allow { get; set; }

		public string Exclude { get; set; }

		public string Prefix { get; set; }

		public object Bind(HttpContextBase httpContext, ParameterDescriptor descriptor)
		{
			var binder = new DataBinder();

			var node = new TreeBuilder().BuildSourceNode(httpContext.Request.Params);

			return binder.BindObject(descriptor.Type, Prefix ?? descriptor.Name, Exclude, Allow, node);
		}
	}
}
