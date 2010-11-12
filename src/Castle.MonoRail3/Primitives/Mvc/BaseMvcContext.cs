namespace Castle.MonoRail3.Primitives.Mvc
{
    public abstract class BaseMvcContext
    {
        protected BaseMvcContext(string areaName, string controllerName, string actionName)
        {
            AreaName = areaName;
            ControllerName = controllerName;
            ActionName = actionName;
        }

        // Kind of copy constructor
        protected BaseMvcContext(BaseMvcContext copy) :
            this(copy.AreaName, copy.ControllerName, copy.ActionName)
        {
        }

        public string AreaName { get; private set; }

        public string ControllerName { get; private set; }
        
		public string ActionName { get; private set; }
    }
}