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
    using System.Collections.Generic;
    using System.IO;
    using Castle.MonoRail.Helpers;
    using Castle.MonoRail.ViewEngines;
    using NUnit.Framework;

    [TestFixture]
    public class GenFormBuilderTests : HelperTestsBase
    {
        private ModelMetadata _metadata;
        private StringWriter _writer;

        private FormTagHelper CreateFormTagHelper()
        {
            return  new FormTagHelper(_ctx);
        }

        [SetUp]
        public override void Init()
        {
            base.Init();

            _metadata = new ModelMetadata();
            _writer = new StringWriter();
        }

        [Test]
        public void FieldFor_DataText_CreatesInputText()
        {
            var customer = new Customer();
            var builder = new GenFormBuilder<Customer>("customer", _writer, CreateFormTagHelper(), customer, _metadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value="""" id=""customer_name""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }

        [Test]
        public void FieldFor_WithValue_CreatesInputWithValue()
        {
            var customer = new Customer() { Name = "hammett" };
            var builder = new GenFormBuilder<Customer>("customer", _writer, CreateFormTagHelper(), customer, _metadata);

            Assert.AreEqual(@"<input type=""text"" name=""customer[name]"" value=""hammett"" id=""customer_name""/>", builder.FieldFor(c => c.Name).ToHtmlString());
        }


        class Customer
        {
            public string Name { get; set; }
        }
    }
}
