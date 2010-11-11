namespace Castle.MonoRail3.Tests.Hosting.Mvc.Typed.Stubs
{
	using Castle.MonoRail3.Primitives.Mvc;

	public class TestActionResult : ActionResult
	{
		public bool executed;

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			executed = true;
		}
	}
}