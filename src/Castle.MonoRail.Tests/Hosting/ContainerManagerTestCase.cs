//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
namespace Castle.MonoRail.Tests.Hosting
{
	using System;
	using System.ComponentModel.Composition;
	using MonoRail.Hosting.Internal;
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
