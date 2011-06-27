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

        [Test]
        public void FieldFor_DataText_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void FieldFor_RequiredDataText_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(() =>
                new Dictionary<PropertyInfo, ModelMetadata>()
                    {
                        { typeof(Customer).GetProperty("Name"), new ModelMetadata { Required = new RequiredAttribute() { } } }
                    });
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name"" required aria-required=""true""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void FieldFor_WithDefaultValue_CreatesInputText()
        {
            var customer = new Customer();
            var modelmetadata = BuildMetadataFor<Customer>(() =>
                new Dictionary<PropertyInfo, ModelMetadata>()
                    {
                        { typeof(Customer).GetProperty("Name"), new ModelMetadata { DefaultValue = "def val" } }
                    });
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, modelmetadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value="""" placeholder=""def val"" id=""customer_name""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void FieldFor_WithValue_CreatesInputWithValue()
        {
            var customer = new Customer() { Name = "hammett" };
            var metadata = BuildMetadataFor<Customer>(null);
            var builder = new GenFormBuilder<Customer>("customer", _writer, _tagHelper, customer, metadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value=""hammett"" id=""customer_name""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }

        private static ModelMetadata BuildMetadataFor<T>(Func<Dictionary<PropertyInfo, ModelMetadata>> buildDict)
        {
            var dict = new Dictionary<PropertyInfo, ModelMetadata>();
            if (buildDict != null)
            {
                dict = buildDict();
            }
            foreach(var info in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!dict.ContainsKey(info))
                {
                    dict[info] = new ModelMetadata();
                }
            }

            return new ModelMetadata(null, dict);
        }

        class Customer
        {
            public string Name { get; set; }
        }
    }
}
