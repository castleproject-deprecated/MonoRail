namespace Castle.MonoRail.Tests
{
	using System;
	using System.Collections.Generic;
	using Hosting.Mvc.Typed;

	public class StubDescriptorProvider : FilterDescriptorProvider
	{
		private readonly List<FilterDescriptor> _descriptors;

		public StubDescriptorProvider(params FilterDescriptor[] descriptors)
		{
			_descriptors = new List<FilterDescriptor>(descriptors);
		}

		public List<FilterDescriptor> Descriptors
		{
			get { return _descriptors; }
		}

		public override IEnumerable<FilterDescriptor> GetDescriptors(ActionExecutionContext context)
		{
			return _descriptors;
		}
	}
}
