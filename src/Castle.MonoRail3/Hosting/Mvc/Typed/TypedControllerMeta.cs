namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using Primitives;
	using Primitives.Mvc;

	public class TypedControllerMeta : ControllerMeta
	{
		public ControllerDescriptor ControllerDescriptor { get; private set; }

		public TypedControllerMeta(object controller, ControllerDescriptor controllerDescriptor) : base(controller)
		{
			ControllerDescriptor = controllerDescriptor;
		}
	}
}
