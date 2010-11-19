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
	using System;
	using System.ComponentModel.Composition;
	using System.Linq;
	using MonoRail.Hosting.Mvc;
	using MonoRail.Hosting.Mvc.ControllerExecutionSink;
	using MonoRail.Hosting.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class TypedControllerExecutorTestCase
	{
		private ActionSink actionSink;
		private AuthSink authSink;
		private TypedControllerExecutor executor;

		[SetUp]
		public void Init()
		{
			actionSink = new ActionSink();
			authSink = new AuthSink();

			executor = new TypedControllerExecutor(GetFactory<IActionResolutionSink>(actionSink),
			                                       GetFactory<IAuthorizationSink>(authSink),
			                                       GetFactory<IPreActionExecutionSink>(),
			                                       GetFactory<IActionExecutionSink>(),
			                                       GetFactory<IActionResultSink>());

			executor.Meta = new TypedControllerMeta(null, null);
		}

		[Test]
		public void Process_should_invoke_execution_sink()
		{
			executor.Process(null);

			Assert.IsTrue(actionSink.invoked);
		}

		[Test]
		public void BuildSink_should_create_a_simple_execution_sink()
		{
			var sink = executor.BuildControllerExecutionSink();

			Assert.AreSame(actionSink, sink);
		}

		//TODO: Add more tests for the controller execution sink construction

		//TODO: Pay attention on this ExportFactory ut dependency
		private static ExportFactory<T>[] GetFactory<T>(params T[] sinks)
		{
			return sinks.Select(s => new ExportFactory<T>(() => new Tuple<T, Action>(s, () => {}))).ToArray();
		}

		public class ActionSink : IActionResolutionSink
		{
			public bool invoked;

			public IControllerExecutionSink Next { get; set; }

			public void Invoke(ControllerExecutionContext executionCtx)
			{
				invoked = true;
			}
		}

		public class AuthSink : IAuthorizationSink
		{
			public IControllerExecutionSink Next { get; set; }

			public void Invoke(ControllerExecutionContext executionCtx)
			{
			}
		}
	}
}
