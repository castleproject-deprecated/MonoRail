namespace TestWebApp.Controller
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Web;
    using Castle.MonoRail;

    [Export]
    public class ContentNegotiator
    {
        private readonly HttpRequestBase _requestBase;

        public ContentNegotiator(HttpRequestBase requestBase)
        {
            _requestBase = requestBase;
        }

        public void RespondTo(params ContentType[] types)
        {
        }

        public ActionResult Respond(Action<RequestFormat> eval)
        {
            // Headers to care about
            // Accept
            // If-Match
            // If-Modified-Since
            // If-None-Match
            // If-Unmodified-Since

            var acceptance = _requestBase.AcceptTypes;

            if (acceptance.Length == 1)
            {
                // fx is requesting
                var selector = acceptance[0];

                var format = new RequestFormat();
                eval(format);
            }

            return null;
        }
    }

    public class ContentType
    {
        public static readonly ContentType Html = new ContentType("text/html");
        public static readonly ContentType Xml = new ContentType("application/xml");
        public static readonly ContentType JSon = new ContentType("application/json");
        public static readonly ContentType Js = new ContentType("text/javascript");

        public ContentType(string mimetype)
        {
            Mimetype = mimetype;
        }

        public string Mimetype { get; private set; }

        public bool Equals(ContentType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Mimetype, Mimetype);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ContentType)) return false;
            return Equals((ContentType) obj);
        }

        public override int GetHashCode()
        {
            return (Mimetype != null ? Mimetype.GetHashCode() : 0);
        }
    }

//    public class RespondToAttribute : Attribute
//    {
//    }

    public class RequestFormat
    {
        private readonly Dictionary<ContentType, Func<ActionResult>> _ct2Func = new Dictionary<ContentType, Func<ActionResult>>();

        public RequestFormat()
        {
        }

        public void Html()
        {
            Add(ContentType.Html, () => new ViewResult());
        }
        public void Html(Func<ActionResult> eval)
        {
            Add(ContentType.Html, eval);
        }

        public void Xml()
        {
            Add(ContentType.Xml, () => new XmlResult());
        }
        public void Xml(Func<ActionResult> eval)
        {
            Add(ContentType.Xml, eval);
        }
        
        public void JSon()
        {
            Add(ContentType.JSon, () => new JSonResult());
        }
        public void JSon(Func<ActionResult> eval)
        {
            Add(ContentType.JSon, eval);
        }

        public void Add(ContentType contentType, Func<ActionResult> eval)
        {
            EnsureCanSetForMimeType(contentType);
            _ct2Func[contentType] = eval;
        }

        private void EnsureCanSetForMimeType(ContentType contentType)
        {
            if (_ct2Func.ContainsKey(contentType))
            {
                throw new InvalidOperationException();
            }
        }
    }
}