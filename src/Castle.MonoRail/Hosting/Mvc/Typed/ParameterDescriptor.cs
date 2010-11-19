namespace Castle.MonoRail.Hosting.Mvc.Typed
{
	using System;

	public class ParameterDescriptor
	{
		public ParameterDescriptor(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		public string Name { get; set; }

		public Type Type { get; set; }
	}
}