namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using Castle.MonoRail;

	class StubModel : ODataModel
	{
		public StubModel(Action<ODataModel> modelFn) : base("TestNamespace", "TestContainerName")
		{
			if (modelFn != null)
			{
				modelFn(this);
			}
		}
	}
}