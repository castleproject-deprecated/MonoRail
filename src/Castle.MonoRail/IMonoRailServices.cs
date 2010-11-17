namespace Castle.MonoRail
{
	using Hosting.Mvc;

	public interface IMonoRailServices
	{
		CompositeViewEngine ViewEngines { get; }
	}
}
