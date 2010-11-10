namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using System.ComponentModel.Composition;
	using Castle.MonoRail3.Primitives;
	using Castle.MonoRail3.Primitives.ControllerExecutionSink;

	[Export(typeof(IActionExecutionSink))]
	public class ActionExecutionSink : BaseControllerExecutionSink, IActionExecutionSink
	{
		public override void Invoke(ControllerExecutionContext executionCtx)
		{
			var result = executionCtx.SelectedAction.Action(executionCtx.Controller, new object[0]);

			executionCtx.InvocationResult = result;

			Proceed(executionCtx);
		}
	}
}
