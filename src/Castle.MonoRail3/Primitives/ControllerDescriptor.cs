namespace Castle.MonoRail3.Primitives
{
	using System;
	using System.Collections.Generic;

	public class ControllerDescriptor
	{
		public ControllerDescriptor(Type controllerType, string name, string area)
		{
			ControllerType = controllerType;
			Name = name;
			Area = area;
			Actions = new List<ActionDescriptor>();
		}

		public Type ControllerType { get; private set; }
		
		public string Name { get; private set; }
		
		public string Area { get; private set; }
		
		public ICollection<ActionDescriptor> Actions { get; private set; }
	}
}
