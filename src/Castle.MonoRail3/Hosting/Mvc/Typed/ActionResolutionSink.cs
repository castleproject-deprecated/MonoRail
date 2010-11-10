namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using System;
	using System.Linq;
	using System.ComponentModel.Composition;
	using System.Web;
	using Castle.MonoRail3.Primitives;
	using Castle.MonoRail3.Primitives.ControllerExecutionSink;

	[Export(typeof(IActionResolutionSink))]
    public class ActionResolutionSink : BaseControllerExecutionSink, IActionResolutionSink
    {
        public override void Invoke(ControllerExecutionContext executionCtx)
        {
            var action = executionCtx.RouteData.GetRequiredString("action");

            var selectedActions = 
                executionCtx.ControllerDescriptor.Actions.
                    Where(ad => string.Compare(ad.Name, action, StringComparison.OrdinalIgnoreCase) == 0).ToList();

            if (selectedActions.Count > 1)
            {
                //TODO: disambiguation here?
            }
            else
            {
                var selectedAction = selectedActions.FirstOrDefault();
                
                if (selectedAction == null)
                    throw new HttpException(404, "'action' not found");

                executionCtx.SelectedAction = selectedAction;
            }

            Proceed(executionCtx);
        }
    }
}
