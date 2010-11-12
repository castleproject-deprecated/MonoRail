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
namespace Castle.MonoRail3.Tests.Hosting.Mvc.Typed
{
	using System.Web.Routing;
	using MonoRail3.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives.Mvc;
	using Stubs;

	[TestFixture]
	public class ActionExecutionSinkTestCase
	{
		private bool invoked;

		[Test]
		public void Invoke_should_execute_the_selected_action()
		{
			var sink = new ActionExecutionSink();

			var context = new ControllerExecutionContext(null, this, new RouteData(), null)
			              	{
			              		SelectedAction = new TestActionDescriptor(TheAction)
			              	};

			sink.Invoke(context);

			Assert.IsTrue(invoked);
		}

		public object TheAction(object target, object[] args)
		{
			invoked = true;

			return new object();
		}
	}
}
