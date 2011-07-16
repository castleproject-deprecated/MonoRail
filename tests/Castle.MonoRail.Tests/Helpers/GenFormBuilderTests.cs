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
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Reflection;
    using Castle.MonoRail.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class GenFormBuilderTests : HelperTestsBase
    {
        private StringWriter _writer;
        private FormTagHelper _tagHelper;

        [SetUp]
        public override void Init()
        {
            base.Init();

            _writer = new StringWriter();
            _tagHelper = new FormTagHelper(_ctx);
        }

        private ModelMetadataProvider CreateProvider()
        {
            return new StubModelMetadataProvider(t => BuildMetadataFor(t, null));
        }

        [Test]
        public void EditorFor_DataText_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name""/>", 
                builder.EditorFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void EditorFor_RequiredDataText_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(() =>
                new Dictionary<PropertyInfo, ModelMetadata>()
                    {
                        { typeof(Customer).GetProperty("Name"), new ModelMetadata(typeof(Customer), typeof(Customer).GetProperty("Name")) { Required = new RequiredAttribute() { } } }
                    });
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name"" required aria-required=""true""/>",
                builder.EditorFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void EditorFor_WithDefaultValue_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(() =>
                new Dictionary<PropertyInfo, ModelMetadata>()
                    {
                        { typeof(Customer).GetProperty("Name"), 
                            new ModelMetadata(typeof(Customer), typeof(Customer).GetProperty("Name")) { DefaultValue = "def val" } }
                    });
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value="""" placeholder=""def val"" id=""customer_name""/>", 
                builder.EditorFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void EditorFor_WithValue_CreatesInputWithValue()
        {
            var customer = new Customer() { Name = "hammett" };
            var metadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, metadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value=""hammett"" id=""customer_name""/>", 
                builder.EditorFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void EditorFor_WithValue_CreatesInputWithValueEncoded()
        {
            var customer = new Customer() { Name = "hammett <ver>" };
            var metadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, metadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[name]"" value=""hammett &lt;ver&gt;"" id=""customer_name""/>",
                builder.EditorFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void EditorFor_DepthOfPropertiesAccess_CreatesInputMatchingProperties()
        {
            var customer = new Customer() { Name = "hammett", LinkedUser = new User() { Email = "h@some.com" } };
            var metadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, metadata, CreateProvider());

            Assert.AreEqual(
                @"<input type=""text"" name=""customer[linkeduser][email]"" value=""h@some.com"" id=""customer_linkeduser_email""/>",
                builder.EditorFor(c => c.LinkedUser.Email).ToHtmlString());
        }

        class User
        {
            public string Email { get; set; }
        }

        class Customer
        {
            public string Name { get; set; }
            public User LinkedUser { get; set; }
        }
    }
}
