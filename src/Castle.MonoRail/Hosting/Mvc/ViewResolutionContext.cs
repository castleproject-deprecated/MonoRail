using Castle.MonoRail.Primitives.Mvc;

namespace Castle.MonoRail.Hosting.Mvc
{
    public class ViewResolutionContext : BaseMvcContext
    {
        public ViewResolutionContext(BaseMvcContext copy) : base(copy)
        {
        }

        public ViewResolutionContext(string areaName, string controllerName, string actionName) : 
            base(areaName, controllerName, actionName)
        {
        }
    }
}