namespace TestWebApp.Controller
{
    using System;
    using System.Web;
    using Castle.MonoRail;

    // optional
    // [RespondTo()]
    public class IssuesController
    {
        private readonly ContentNegotiator _contentNegotiator;

        public IssuesController(ContentNegotiator contentNegotiator)
        {
            _contentNegotiator = contentNegotiator;
            // optional
        }

        public ActionResult Get(int id, HttpResponseBase response)
        {


            return _contentNegotiator.Respond(format =>
                                                  {
                                                      format.Html();
                                                      format.JSon();
                                                  }
                );

        }

    }











    
}