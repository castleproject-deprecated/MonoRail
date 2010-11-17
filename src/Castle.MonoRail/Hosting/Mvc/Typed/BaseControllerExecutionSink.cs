namespace Castle.MonoRail.Hosting.Mvc.Typed
{
	using Primitives.Mvc;

	public abstract class BaseControllerExecutionSink : IControllerExecutionSink
	{
		public IControllerExecutionSink Next { get; set; }

		public abstract void Invoke(ControllerExecutionContext executionCtx);

		protected void Proceed(ControllerExecutionContext executionCtx)
		{
			if (Next != null)
				Next.Invoke(executionCtx);
		}
	}
}