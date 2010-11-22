namespace TestWebApp.Controller
{
    using System.Web;
    using Castle.MonoRail;
    using Castle.MonoRail.Mvc.Rest;

    // optional
    // [RespondTo()]
    public class IssuesController
    {
        private readonly ContentNegotiator _contentNegotiator;

        public IssuesController(ContentNegotiator contentNegotiator)
        {
            _contentNegotiator = contentNegotiator;
            // _contentNegotiator.Allow();
        }

        // [HttpVerbs()]
        public ActionResult Get(int id, HttpResponseBase response)
        {
            return _contentNegotiator.Respond(format =>
                                                  {
                                                      format.Html();
                                                      format.JSon();
                                                  });
        }
    }
}