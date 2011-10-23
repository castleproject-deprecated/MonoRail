namespace Castle.MonoRail.Views.Brail.TestSite.Controllers
{
    using System;

    using Castle.MonoRail.Framework;

    [Serializable, Layout("dsl")]
    public class DslController : SmartDispatcherController
    {
        public void TestSubViewOutput()
        {
            PropertyBag["SayHelloTo"] = "Harris";
        }

        public void TestSubViewWithComponents()
        {
            PropertyBag["items"] = new[]
            {
                "Ayende",
                "Rahien"
            };            
        }

        public void TestXml()
        {
            PropertyBag["items"] = new[]
            {
                "Ayende",
                "Rahien"
            };
        }
    }
}
