#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
    using Castle.MonoRail.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class JsonHelperTests : HelperTestsBase
    {
        private JsonHelper _jsonHelper;

        [SetUp]
        public override void Init()
        {
            base.Init();
            _jsonHelper = new JsonHelper(_helperContext);
        }

        [Test]
        public void ToJson_ForGraph_SerializesToJson()
        {
            var model = new Customer() {Name = "hammett"};
            var json = _jsonHelper.ToJson(model);

            Assert.AreEqual(@"{""Name"":""hammett""}", json.ToHtmlString());
        }

        class Customer
        {
            public string Name { get; set; }
        }
    }
}
