namespace Castle.MonoRail3.Primitives
{
	using System.Collections.Generic;

	public class ControllerMeta
    {
        public ControllerMeta(object controller)
        {
            Metadata = new Dictionary<string, object>();
            ControllerInstance = controller;
        }

        public IDictionary<string, object> Metadata { get; set; }

        public object ControllerInstance { get; set; }
    }
}
