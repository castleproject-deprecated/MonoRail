namespace Castle.MonoRail3
{
	using Hosting.Mvc;

	public interface IMonoRailServices
	{
		CompositeViewEngine ViewEngines { get; }
	}
}
