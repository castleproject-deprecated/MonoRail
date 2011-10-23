#region License
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
#endregion
namespace Castle.MonoRail.Tests.Mvc.Typed
{
	using System.Web.Routing;
	using Fakes;
	using Internal;
	using MonoRail.Mvc.Typed;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ReflectionBasedControllerProviderTestCase
	{
		[Test]
		public void Create_should_create_requested_controller_instance()
		{
			var hosting = new Mock<IHostingBridge>();
			var data = new RouteData();
			data.Values.Add("controller", "sometest");

			hosting.SetupGet(bridge => bridge.ReferencedAssemblies).Returns(new[] {GetType().Assembly});

			var provider = new ReflectionBasedControllerProvider(hosting.Object)
			               	{
			               		DescriptorBuilder = new ControllerDescriptorBuilder()
			               	};

			var meta = provider.Create(data);

			Assert.IsNotNull(meta);
			Assert.IsInstanceOf<SomeTestController>(meta.ControllerInstance);
		}
	}
}
