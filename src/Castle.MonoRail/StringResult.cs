namespace Castle.MonoRail
{
	using Primitives.Mvc;

	public class StringResult : ActionResult
	{
		private readonly string output;

		public StringResult(string output)
		{
			this.output = output;
		}

		public override void Execute(ActionResultContext context, IMonoRailServices services)
		{
			context.HttpContext.Response.Write(output);
		}
	}
}
