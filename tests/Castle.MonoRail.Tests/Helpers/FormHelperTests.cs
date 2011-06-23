namespace Castle.MonoRail.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    [TestFixture]
    public class FormHelperTests
    {
        private IDictionary<string, object> _viewBag;
        private FormHelper _formHlpr;
        private ViewContext _ctx;
        private StubTargetUrl _url;

        [SetUp]
        public void CreateFormHelper()
        {
            _viewBag = new Dictionary<string, object>();
            var viewReq = new ViewRequest();
            _ctx = new ViewContext(null, _viewBag, new object(), viewReq);

            _formHlpr = new FormHelper(_ctx);
            _url = new StubTargetUrl();
        }

        [Test]
        public void FormForGeneric_UseOfExpressionForProperty_GeneratesCorrectInputMarkup()
        {
            var html = 
                _formHlpr.FormFor(new Customer(), _url, 
                (builder) =>
                {
                    return new HtmlResult(writer =>
                    {
                        writer.Write("<p>");
                        writer.Write(builder.FieldFor(c => c.Name).ToHtmlString());
                        writer.Write("</p>");
                    });
                });

            Assert.AreEqual(
@"<form action=""/url/generated"" method=""post"" >
<p><input type=""text"" name=""customer_name"" id=""Customer[Name]"" /></p>
</form>", 
                    html.ToString());
        }

        [Test]
        public void FormFor_UseOfFormBuilder_GeneratesCorrectInputMarkup()
        {
            var html =
                _formHlpr.FormFor(_url,
                (builder) =>
                {
                    return new HtmlResult(writer =>
                    {
                        writer.Write("<p>");
                        writer.Write(builder.TextField("name").ToHtmlString());
                        writer.Write("</p>");
                    });
                });

            Assert.AreEqual(
@"<form action=""/url/generated"" method=""post"" >
<p><input type=""text"" name=""name"" id=""name"" /></p>
</form>",
                    html.ToString());
        }






        class Customer
        {
            public string Name { get; set; }
        }

        class StubTargetUrl : TargetUrl
        {
            public override string Generate(IDictionary<string, string> parameters)
            {
                return "/url/generated";
            }
        }
    }
}
