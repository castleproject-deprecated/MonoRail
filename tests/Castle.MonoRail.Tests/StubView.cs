namespace Castle.MonoRail.Tests
{
    using System.IO;
    using Castle.MonoRail.ViewEngines;

    public class StubView : IView
    {
        public ViewContext _ctx;

        public void Process(TextWriter writer, ViewContext ctx)
        {
            _ctx = ctx;
        }
    }
}
