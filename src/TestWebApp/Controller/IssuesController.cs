namespace TestWebApp.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Castle.MonoRail;
    using Domain;

    // optional
    // [RespondTo()]
    public class IssuesController
    {
        private readonly ContentNegotiator _contentNegotiator;

        public IssuesController(ContentNegotiator contentNegotiator)
        {
            _contentNegotiator = contentNegotiator;
            // optional
            // _contentNegotiator.RespondTo(ContentType.Html), ContentType.JSon, ContentType.Xml);
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

//        public IEnumerable<Issue> List()
//        {
//            return new Issue[]
//                       {
//                           new Issue(),
//                       } ;
//        }
    }











    public class ContentType
    {
        public static readonly ContentType Html = new ContentType();
        public static readonly ContentType Xml = new ContentType();
        public static readonly ContentType JSon = new ContentType();
    }

    public class RespondToAttribute : Attribute
    {
    }

    public class RequestFormat
    {
        public void Html()
        {

        }
        public void Html(Func<ActionResult> eval)
        {
            eval();
        }
        public void Xml()
        {

        }
        public void Xml(Func<ActionResult> eval)
        {
            eval();
        }
        public void JSon()
        {

        }
        public void JSon(Func<ActionResult> eval)
        {
            eval();
        }
    }

    public class ContentNegotiator
    {
        public void RespondTo(params ContentType[] types)
        {
        }

        public ActionResult Respond(Action<RequestFormat> eval)
        {
            return null;
        }
    }
}