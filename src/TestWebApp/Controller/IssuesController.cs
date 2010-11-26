namespace TestWebApp.Controller
{
    using System.Collections.Generic;
    using Castle.MonoRail;
    using Model;

    public class IssuesController
    {
        public IssuesController()
        {
        }

        public ActionResult Get(int id)
        {
        	return new StringResult("foo");
        }

        public IEnumerable<Issue> List()
        {
            return new Issue[]
                       {
                           new Issue(),
                       } ;
        }
    }
}