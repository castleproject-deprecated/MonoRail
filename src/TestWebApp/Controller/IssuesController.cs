namespace TestWebApp.Controller
{
    using System.Collections.Generic;
    using Model;

    public class IssuesController
    {
        public IEnumerable<Issue> List()
        {
            return new Issue[]
                       {
                           new Issue(),
                       } ;
        }
    }
}