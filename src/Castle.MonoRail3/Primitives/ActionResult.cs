namespace Castle.MonoRail3.Primitives
{
	using MonoRail;

	public abstract class ActionResult
    {
        public abstract void Execute(ActionResultContext context, IMonoRailServices services);
    }
}
