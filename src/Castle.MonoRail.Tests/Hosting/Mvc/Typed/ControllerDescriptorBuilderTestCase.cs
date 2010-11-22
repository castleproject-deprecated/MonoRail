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
namespace Castle.MonoRail.Tests.Hosting.Mvc.Typed
{
	using System.Linq;
	using Castle.MonoRail.Mvc.Typed;
	using Fakes;
	using NUnit.Framework;

	[TestFixture]
	public class ControllerDescriptorBuilderTestCase
	{
		[Test]
		public void Build_should_inspect_controller_type_to_collect_and_normalize_name()
		{
			var builder = new ControllerDescriptorBuilder();

			var descriptor = builder.Build(typeof (SomeTestController));

			Assert.AreEqual("sometest", descriptor.Name);
		}

		[Test]
		public void Build_should_inspect_controller_type_to_collect_actions()
		{
			var builder = new ControllerDescriptorBuilder();

			var descriptor = builder.Build(typeof(SomeTestController));

			Assert.IsTrue(descriptor.Actions.Any(a => a.Name == "Index"));
		}
	}
}
