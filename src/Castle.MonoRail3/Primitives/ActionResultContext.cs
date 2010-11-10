namespace Castle.MonoRail3.Primitives
{
	public class ActionResultContext : BaseMvcContext
    {
        public ActionResultContext(string areaName, string controllerName, string actionName) : 
            base(areaName, controllerName, actionName)
        {
        }
    }
}