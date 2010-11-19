namespace Castle.MonoRail
{
    using System;
    using Hosting.Mvc;

	public interface IMonoRailServices : IServiceProvider
	{
		CompositeViewEngine ViewEngines { get; }


	}
}
