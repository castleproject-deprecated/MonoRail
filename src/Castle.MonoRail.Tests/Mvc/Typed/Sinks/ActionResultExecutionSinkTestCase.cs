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

namespace Castle.MonoRail.Tests.Mvc.Typed.Sinks
{
	using System.Web.Routing;
	using Castle.MonoRail.Mvc;
	using Castle.MonoRail.Tests.Mvc.Typed.Fakes;
	using Castle.MonoRail.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class ActionResultExecutionSinkTestCase
	{
		[Test]
		public void Invoke_should_execute_ActionResult_if_present()
		{
		    var controllerCtx = new ControllerContext();
			var descriptor = new ControllerDescriptor(GetType(), "TestController", "Test");
			var sink = new ActionResultExecutionSink();
			var result = new TestActionResult();
            var context = new ControllerExecutionContext(null, controllerCtx, this, new RouteData(), descriptor)
			              	{
			              		InvocationResult = result,
								SelectedAction = new TestActionDescriptor()
			              	};

			sink.Invoke(context);

			Assert.IsTrue(result.executed);
		}
	}
}
