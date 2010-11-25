namespace TestWebApp.Controller
{
    using System.Web;
    using Castle.MonoRail;
    using Castle.MonoRail.Mvc;
    using Castle.MonoRail.Mvc.Rest;
    using TestWebApp.Domain;

    // optional [RespondTo()]
    public class IssuesController
    {
        private readonly ContentNegotiator _contentNegotiator;
        private readonly ControllerContext _ctx;

        public IssuesController(ContentNegotiator contentNegotiator, ControllerContext controllerContext)
        {
            _contentNegotiator = contentNegotiator;
            _ctx = controllerContext;
            // _contentNegotiator.Allow();
        }

        // [HttpVerbs()]
        public ActionResult Index(int id)
        {
            var issue = new Issue() { Id = id, Title = "Some error"} ;
            _ctx.Data.MainModel = new Resource<Issue>(issue);

            return _contentNegotiator.Respond(format =>
                                                  {
                                                      format.Html();
                                                      format.JSon();
                                                      format.Xml();
                                                  });
        }
    }
}
