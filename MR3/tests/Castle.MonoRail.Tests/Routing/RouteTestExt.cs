namespace Castle.MonoRail.Routing.Tests
{
	using System;

	public static class RouteTestExt
	{
		[System.Diagnostics.DebuggerStepThrough]
        public static RouteMatch TryMatch(this Router router, string path)
        {
            return router.TryMatch(new RequestInfo(path, new Uri("http://localhost:3333/"), ""));
        }

		[System.Diagnostics.DebuggerStepThrough]
		public static RouteMatch TryMatch(this Router router, string path, string vpath)
        {
			return router.TryMatch(new RequestInfo(path, new Uri("http://localhost:3333/"), vpath));
        }
    }
}