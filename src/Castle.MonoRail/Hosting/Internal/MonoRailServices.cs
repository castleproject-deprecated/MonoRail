namespace Castle.MonoRail.Hosting.Internal
{
	using System.ComponentModel.Composition;
	using Castle.MonoRail;
	using Mvc;

	[Export(typeof(IMonoRailServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonoRailServices : IMonoRailServices
    {
        [Import]
        public CompositeViewEngine ViewEngines { get; set; }
    }
}
