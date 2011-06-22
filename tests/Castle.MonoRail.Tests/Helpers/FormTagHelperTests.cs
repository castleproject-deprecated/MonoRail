namespace Castle.MonoRail.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    [TestFixture]
    public class FormTagHelperTests
    {
        private IDictionary<string, object> _viewBag;
        private FormTagHelper _formTagHlpr;
        private ViewContext _ctx;
        private StubTargetUrl _url;
        private HttpContextStub _httpCtx;

        [SetUp]
        public void CreateFormTagHelper()
        {
            _viewBag = new Dictionary<string, object>();
            var viewReq = new ViewRequest();
            _httpCtx = new HttpContextStub();
            _ctx = new ViewContext(_httpCtx, _viewBag, new object(), viewReq);

            _formTagHlpr = new FormTagHelper(_ctx);
            _url = new StubTargetUrl();
        }

        [Test]
        public void FormTag_UrlParameter_GeneratesFormForUrlWithPostMethod()
        {
            Assert.AreEqual(
                @"<form action=""/url/generated"" method=""post"">", 
                _formTagHlpr.FormTag(new StubTargetUrl()).ToHtmlString());
        }

        [Test]
        public void FormTag_NoParameters_GeneratesFormForCurrentPathWithPostMethod()
        {
            _httpCtx.RequestStub._SetPath("/account/new");
            Assert.AreEqual(
                @"<form action=""/account/new"" method=""post"">", 
                _formTagHlpr.FormTag().ToHtmlString());
        }

        [Test]
        public void TextFieldTag_GivenName_GeneratesInputTextMarkup()
        {
            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name""/>",
                _formTagHlpr.TextFieldTag("customer[name]", id: null, value: null).ToHtmlString());
        }

        [Test]
        public void TextFieldTag_GivenNameAndId_GeneratesInputTextMarkup()
        {
            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" id=""cust_name""/>",
                _formTagHlpr.TextFieldTag("customer[name]", "cust_name", value: null).ToHtmlString());
        }

        [Test]
        public void TextFieldTag_GivenRequired_GeneratesInputTextMarkup()
        {
            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" id=""cust_name"" required aria-required=""true""/>",
                _formTagHlpr.TextFieldTag("customer[name]", "cust_name", value: null, required:true, html:null).ToHtmlString());
        }

        class StubTargetUrl : TargetUrl
        {
            public override string Generate(IDictionary<string, string> parameters)
            {
                return "/url/generated";
            }
        }

        private class HttpContextStub : HttpContextBase
        {
            public class HttpRequestStub : HttpRequestBase
            {
                private string _path;

                public void _SetPath(string v)
                {
                    _path = v;
                }
                public override string Path
                {
                    get { return _path; }
                    
                }
            }
            public class HttpResponseBaseStub : HttpResponseBase
            {
            }
            public class HttpServerUtilityStub : HttpServerUtilityBase
            {
                public override string HtmlEncode(string s)
                {
                    return HttpUtility.HtmlEncode(s);
                }
            }

            public HttpRequestStub RequestStub = new HttpRequestStub();
            public HttpResponseBaseStub ResponseStub = new HttpResponseBaseStub();
            public HttpServerUtilityStub ServerUtilityStub = new HttpServerUtilityStub();

            public override HttpRequestBase Request
            {
                get { return RequestStub; }
            }

            public override HttpResponseBase Response
            {
                get { return ResponseStub; }
            }

            public override HttpServerUtilityBase Server
            {
                get { return ServerUtilityStub; }
            }
        }
    }
}
