namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Diagnostics.Contracts;
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail3.Primitives;
	using Castle.MonoRail3.Primitives.ControllerExecutionSink;

	[Export]
    public class TypedControllerExecutor : ControllerExecutor
    {
        private readonly ExportFactory<IActionResolutionSink>[] _firstSinksFactory;
        private readonly ExportFactory<IAuthorizationSink>[] _secondSinksFactory;
        private readonly ExportFactory<IPreActionExecutionSink>[] _thirdSinksFactory;
        private readonly ExportFactory<IActionExecutionSink>[] _forthSinksFactory;
        private readonly ExportFactory<IActionResultSink>[] _fifthSinksFactory;

        [ImportingConstructor]
        public TypedControllerExecutor(
            [ImportMany] ExportFactory<IActionResolutionSink>[] firstSinksFactory,
            [ImportMany] ExportFactory<IAuthorizationSink>[] secondSinksFactory,
            [ImportMany] ExportFactory<IPreActionExecutionSink>[] thirdSinksFactory,
            [ImportMany] ExportFactory<IActionExecutionSink>[] forthSinksFactory,
            [ImportMany] ExportFactory<IActionResultSink>[] fifthSinksFactory)
        {
            _firstSinksFactory = firstSinksFactory;
            _secondSinksFactory = secondSinksFactory;
            _thirdSinksFactory = thirdSinksFactory;
            _forthSinksFactory = forthSinksFactory;
            _fifthSinksFactory = fifthSinksFactory;
        }

		public TypedControllerMeta Meta { get; set; }
		public RouteData RouteData { get; set; }

        public override void Process(HttpContextBase context)
        {
            var first = BuildControllerExecutionSink();

        	var invCtx = new ControllerExecutionContext(context, Meta.ControllerInstance, RouteData, Meta.ControllerDescriptor);

            first.Invoke(invCtx);
        }

		public IControllerExecutionSink BuildControllerExecutionSink()
		{
			var first = CreateAndConnectSinks(_fifthSinksFactory, null);

			first = CreateAndConnectSinks(_forthSinksFactory, first);
			first = CreateAndConnectSinks(_thirdSinksFactory, first);
			first = CreateAndConnectSinks(_secondSinksFactory, first);
			first = CreateAndConnectSinks(_firstSinksFactory, first);

			if (first == null)
				//TODO: need exception model
				throw new Exception("No sink for action resolution?");
			
			return first;
		}

		private static IControllerExecutionSink
            CreateAndConnectSinks<T>(ICollection<ExportFactory<T>> list, IControllerExecutionSink previousFirst)
            where T : class, IControllerExecutionSink
        {
            T prev = null;
            T first = null;

            foreach(var sinkFactory in list)
            {
                var sink = sinkFactory.CreateExport().Value;

                if (prev != null)
                    prev.Next = sink;

                if (first == null)
                    first = sink;

                prev = sink;
            }

            if (previousFirst != null && prev != null)
                prev.Next = previousFirst;

            return first ?? previousFirst;
        }
    }
}
