namespace Castle.MonoRail3.Hosting.Internal
{
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Reflection;
	using System.Web.Compilation;

	[Export(typeof(IHostingBridge))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class BuildManagerAdapter : IHostingBridge
	{
		public IEnumerable<Assembly> ReferencedAssemblies
		{
			get { return BuildManager.GetReferencedAssemblies().Cast<Assembly>(); }
		}
	}
}
