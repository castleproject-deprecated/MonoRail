namespace Castle.MonoRail.Hosting.Mvc
{
    using System.IO;
    using MonoRail3.Hosting.Mvc;

	public interface IView
    {
        void Process(ViewContext viewContext, TextWriter writer);
    }
}