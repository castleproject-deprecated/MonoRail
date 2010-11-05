namespace Castle.MonoRail3.Primitives
{
	using System;

	public abstract class ActionDescriptor
	{
		public string Name { get; protected set; }
		
		public Func<object, object[], object> Action { get; protected set; }
	}
}