namespace Castle.MonoRail.Tests
{
    using System;
    using System.IO;
    using Castle.MonoRail.ViewEngines;

    public class StubView : IView
    {
        private readonly Action<TextWriter> _writingAction;

        public StubView()
        {
        }

        public StubView(Action<TextWriter> writingAction)
        {
            _writingAction = writingAction;
        }

        public ViewContext _ctx;

        public void Process(TextWriter writer, ViewContext ctx)
        {
            if (_writingAction != null)
            {
                _writingAction(writer);
            }

            _ctx = ctx;
        }
    }
}
