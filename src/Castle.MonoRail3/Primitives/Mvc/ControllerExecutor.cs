namespace Castle.MonoRail3.Primitives.Mvc
{
    using System.Web;

    public abstract class ControllerExecutor
    {
        public abstract void Process(HttpContextBase context);
    }
}
