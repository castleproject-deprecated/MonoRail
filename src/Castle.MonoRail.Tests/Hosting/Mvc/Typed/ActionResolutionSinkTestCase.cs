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
	using System.Web;
	using System.Web.Routing;
	using Fakes;
	using MonoRail.Hosting.Mvc;
	using MonoRail.Hosting.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class ActionResolutionSinkTestCase
	{
		[Test]
		public void Invoke_should_find_action_on_controller()
		{
			var data = new RouteData();
			data.Values.Add("action", "TestAction");

			var sink = new ActionResolutionSink();
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			descriptor.Actions.Add(new TestActionDescriptor());

			var context = new ControllerExecutionContext(null, this, data, descriptor);

			sink.Invoke(context);

			Assert.IsNotNull(context.SelectedAction);
			Assert.AreEqual("TestAction", context.SelectedAction.Name);
		}

		[Test, ExpectedException(typeof(HttpException))]
		public void Invoke_should_thrown_an_404_if_cannot_find_a_action()
		{
			var data = new RouteData();
			data.Values.Add("action", "Foo");

			var sink = new ActionResolutionSink();
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			descriptor.Actions.Add(new TestActionDescriptor());

			var context = new ControllerExecutionContext(null, this, data, descriptor);

			sink.Invoke(context);
		}
	}
}
