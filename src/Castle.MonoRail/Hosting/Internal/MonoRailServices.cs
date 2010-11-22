namespace Castle.MonoRail.Hosting.Internal
{
    using System;
    using System.ComponentModel.Composition;
	using Castle.MonoRail;
    using Mvc.ViewEngines;

    [Export(typeof(IMonoRailServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonoRailServices : IMonoRailServices
    {
        [Import]
        public CompositeViewEngine ViewEngines { get; set; }

        #region IServiceProvider

        public object GetService(Type serviceType)
	    {
	        throw new NotImplementedException();
        }

        #endregion
    }
}
