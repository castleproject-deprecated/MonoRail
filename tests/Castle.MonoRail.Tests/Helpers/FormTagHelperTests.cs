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
    public class FormTagHelperTests : HelperTestsBase
    {
        private FormTagHelper _formTagHlpr;
        private StubTargetUrl _url;

        [SetUp]
        public override void Init()
        {
            base.Init();

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
    }
}
