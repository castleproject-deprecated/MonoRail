#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    [TestFixture]
    public class FormHelperTests : HelperTestsBase
    {
        private FormHelper _formHlpr;
        private StubTargetUrl _url;

        [SetUp]
        public override void Init()
        {
            base.Init();
            _formHlpr = new FormHelper(_ctx, new StubModelMetadataProvider());
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
@"<form id=""customer_form"" action=""/url/generated"" method=""post"">
<p><input type=""text"" name=""customer[name]"" value="""" id=""customer_name""/></p>
</form>
", html.ToString());
        }

        [Test]
        public void FormFor_UseOfFormBuilder_GeneratesCorrectInputMarkup()
        {
            var html =
                _formHlpr.FormFor(_url, "prefix", 
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
</form>", html.ToString());
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
