namespace Castle.MonoRail3
{
	using Primitives.Mvc;

	public abstract class ActionResult
    {
        public abstract void Execute(ActionResultContext context, IMonoRailServices services);
    }
}
