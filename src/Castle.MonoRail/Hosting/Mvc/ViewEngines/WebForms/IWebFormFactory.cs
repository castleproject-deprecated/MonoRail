namespace Castle.MonoRail.Hosting.Mvc.ViewEngines.WebForms
{
    using System;

    public interface IWebFormFactory
    {
        object CreateInstanceFromVirtualPath(string path, Type baseType);
    }
}
