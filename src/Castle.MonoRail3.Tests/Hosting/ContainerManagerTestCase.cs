namespace Castle.MonoRail3.Tests.Hosting
{
	using System;
	using System.ComponentModel.Composition;
	using MonoRail3.Hosting.Internal;
	using NUnit.Framework;

	[TestFixture]
	public class ContainerManagerTestCase
	{
		[SetUp]
		public void Init()
		{
			ContainerManager.CatalogPath = AppDomain.CurrentDomain.BaseDirectory;
		}

		[Test]
		public void CreateRequestContainer_should_build_container_with_non_shared_and_shared_parts()
		{
			var container = ContainerManager.CreateRequestContainer();

			Assert.IsNotNull(container.GetExport<NonSharedComponent>());
			Assert.IsNotNull(container.GetExport<SharedComponent>());
		}

		[Test]
		public void CreateContainer_should_build_container_with_shared_parts_only()
		{
			var container = ContainerManager.CreateContainer();

			Assert.IsNotNull(container.GetExport<SharedComponent>());
			Assert.Throws(typeof(ImportCardinalityMismatchException), () => container.GetExport<NonSharedComponent>());
		}


		[Export]
		[PartCreationPolicy(CreationPolicy.Shared)]
		public class SharedComponent
		{
			
		}

		[Export]
		[PartCreationPolicy(CreationPolicy.NonShared)]
		public class NonSharedComponent
		{

		}
	}
}
