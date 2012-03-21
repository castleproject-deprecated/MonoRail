namespace Castle.MonoRail.Tests
{
	using System;
	using Framework;

	public class ComponentOrder : IComponentOrder
	{
		public ComponentOrder(int order)
		{
			this.Order = order;
		}

		public int Order { get; set; }
	}
}
