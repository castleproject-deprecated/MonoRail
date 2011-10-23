#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Tests.Hosting
{
	using System;
	using System.ComponentModel.Composition;
	using Internal;
	using NUnit.Framework;

	[TestFixture]
	public class ContainerManagerTestCase
	{
		[SetUp]
		public void Init()
		{
			ContainerManager.CatalogPath = AppDomain.CurrentDomain.BaseDirectory;
		}

//		[Test]
//		public void CreateRequestContainer_should_build_container_with_non_shared_and_shared_parts()
//		{
//			var container = ContainerManager.CreateRequestContainer(TODO);
//
//			Assert.IsNotNull(container.GetExport<NonSharedComponent>());
//			Assert.IsNotNull(container.GetExport<SharedComponent>());
//		}

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
