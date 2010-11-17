namespace Castle.MonoRail
{
	using Primitives.Mvc;

	public abstract class ActionResult
    {
        public abstract void Execute(ActionResultContext context, IMonoRailServices services);
    }
}
