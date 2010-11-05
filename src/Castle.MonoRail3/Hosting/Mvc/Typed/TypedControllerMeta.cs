namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using Castle.MonoRail3.Primitives;

	public class TypedControllerMeta : ControllerMeta
	{
		public ControllerDescriptor ControllerDescriptor { get; private set; }

		public TypedControllerMeta(object controller, ControllerDescriptor controllerDescriptor) : base(controller)
		{
			ControllerDescriptor = controllerDescriptor;
		}
	}
}
