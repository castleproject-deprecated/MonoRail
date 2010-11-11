namespace Castle.MonoRail3.Primitives
{
	using System.Web;

	public abstract class ControllerExecutor
    {
        public abstract void Process(HttpContextBase context);
    }
}
