namespace Castle.MonoRail3.Tests.Hosting.Mvc.Typed
{
	using System;
	using System.ComponentModel.Composition;
	using System.Linq;
	using MonoRail3.Hosting.Mvc.ControllerExecutionSink;
	using MonoRail3.Hosting.Mvc.Typed;
	using NUnit.Framework;
	using Primitives;

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
