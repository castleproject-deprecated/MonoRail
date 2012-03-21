namespace Castle.MonoRail.Routing.Tests
{
    public static class RouteTestExt
	{
		[System.Diagnostics.DebuggerStepThrough]
        public static RouteMatch TryMatch(this Router router, string path)
        {
            return router.TryMatch(new RequestInfoAdapter(path, null, null, null, null));
        }

		[System.Diagnostics.DebuggerStepThrough]
		public static RouteMatch TryMatch(this Router router, string path, string vpath)
        {
            return router.TryMatch(new RequestInfoAdapter(path, null, null, null, vpath));
        }
    }
}