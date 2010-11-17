namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed.Stubs
{
	using Castle.MonoRail.Primitives.Mvc;

	public class TestActionResult : ActionResult
	{
		public bool executed;

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			executed = true;
		}
	}
}