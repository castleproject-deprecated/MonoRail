namespace Castle.MonoRail3.Hosting.Internal
{
	using System.ComponentModel.Composition;
	using Castle.MonoRail3;
	using Mvc;

	[Export(typeof(IMonoRailServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonoRailServices : IMonoRailServices
    {
        [Import]
        public CompositeViewEngine ViewEngines { get; set; }
    }
}
