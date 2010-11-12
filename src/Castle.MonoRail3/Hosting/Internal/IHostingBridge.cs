namespace Castle.MonoRail3.Hosting.Internal
{
	using System.Collections.Generic;
	using System.Reflection;

	public interface IHostingBridge
	{
		IEnumerable<Assembly> ReferencedAssemblies { get; }
	}
}
