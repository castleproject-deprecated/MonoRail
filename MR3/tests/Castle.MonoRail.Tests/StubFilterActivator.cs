namespace Castle.MonoRail.Tests
{
	using System;
	using Hosting.Mvc.Typed;

	public class StubFilterActivator : IFilterActivator
	{
		private readonly Func<Type, object> _creator;

		public StubFilterActivator(Func<Type, object> creator)
		{
			_creator = creator;
		}

		public TFilter Activate<TFilter>(Type filterType) where TFilter : class
		{
			return (TFilter) _creator(filterType);
		}
	}
}
