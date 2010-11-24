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
	using System.Collections.Specialized;
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail.Mvc.Typed;
	using Castle.MonoRail.Mvc;
	using Fakes;
	using MonoRail.Mvc.Typed.Sinks;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ActionExecutionSinkTestCase
	{
		private bool invoked;
		private string _a;
		private int _b;

		[Test]
		public void Invoke_should_execute_the_selected_action()
		{
			var sink = new ActionExecutionSink();

			var context = new ControllerExecutionContext(null, new ControllerContext(), this, new RouteData(), null)
			              	{
			              		SelectedAction = new TestActionDescriptor(FakeAction)
			              	};

			sink.Invoke(context);

			Assert.IsTrue(invoked);
		}

		[Test]
		public void Invoke_should_bind_parameters_using_request_data()
		{
			var http = new Mock<HttpContextBase>();
			var request = new Mock<HttpRequestBase>();
			var sink = new ActionExecutionSink();

			http.SetupGet(ctx => ctx.Request).Returns(request.Object);
			request.SetupGet(r => r.Params).Returns(new NameValueCollection {{"a", "the value"}, {"b", "123"}});

            var context = new ControllerExecutionContext(http.Object, new ControllerContext(), this, new RouteData(), null)
			              	{
								SelectedAction = new MethodInfoActionDescriptor(GetType().GetMethod("WithParametersAction"))
			              	};

			sink.Invoke(context);

			Assert.IsTrue(invoked);
			Assert.AreEqual("the value", _a);
			Assert.AreEqual(123, _b);
		}

		[Test]
		public void Invoke_should_bind_parameters_using_routing_data()
		{
			var http = new Mock<HttpContextBase>();
			var request = new Mock<HttpRequestBase>();
			var sink = new ActionExecutionSink();

			http.SetupGet(ctx => ctx.Request).Returns(request.Object);
			request.SetupGet(r => r.Params).Returns(new NameValueCollection());

			var routeData = new RouteData();
			routeData.Values.Add("a", "other value");
			routeData.Values.Add("b", "123");

			var context = new ControllerExecutionContext(http.Object, new ControllerContext(), this, routeData, null)
			{
				SelectedAction = new MethodInfoActionDescriptor(GetType().GetMethod("WithParametersAction"))
			};

			sink.Invoke(context);

			Assert.IsTrue(invoked);
			Assert.AreEqual("other value", _a);
		}

		public object FakeAction(object target, object[] args)
		{
			invoked = true;

			return new object();
		}

		public object WithParametersAction(string a, int b)
		{
			invoked = true;
			_a = a;
			_b = b;

			return new object();
		}
	}
}
