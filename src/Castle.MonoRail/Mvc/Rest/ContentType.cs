namespace Castle.MonoRail.Mvc.Rest
{
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
}