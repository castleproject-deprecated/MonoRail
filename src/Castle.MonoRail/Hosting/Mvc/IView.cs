namespace Castle.MonoRail.Hosting.Mvc
{
    using System.IO;

	public interface IView
    {
        void Process(ViewContext viewContext, TextWriter writer);
    }
}