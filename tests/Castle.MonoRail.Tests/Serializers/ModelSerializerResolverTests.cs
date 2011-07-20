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

namespace Castle.MonoRail.Tests.Serializers
{
    using System.IO;
    using System.Web;
    using Castle.MonoRail.Serialization;
    using NUnit.Framework;

    [TestFixture]
    public class ModelSerializerResolverTests
    {
        [Test]
        public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForJson()
        {
            var resolver = new ModelSerializerResolver();
            var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
            Assert.IsNotNull(serializer);
        }

        [Test]
        public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForXml()
        {
            var resolver = new ModelSerializerResolver();
            var serializer = resolver.CreateSerializer<Customer>(MimeType.Xml);
            Assert.IsNotNull(serializer);
        }

        [Test]
        public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForFormData()
        {
            var resolver = new ModelSerializerResolver();
            var serializer = resolver.CreateSerializer<Customer>(MimeType.FormUrlEncoded);
            Assert.IsNotNull(serializer);
        }

        [Test, ExpectedException(typeof(MonoRailException))]
        public void CreateSerializer_DefaultSerializers_NoSerializerForJS()
        {
            var resolver = new ModelSerializerResolver();
            resolver.CreateSerializer<Customer>(MimeType.Js);
        }

        [Test]
        public void Register_OfJsonSerializer_TakesPrecedenceOverDefault()
        {
            var resolver = new ModelSerializerResolver();
            resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
            var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
            Assert.IsNotNull(serializer);
            Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());
        }

        [Test]
        public void Register_OfJsSerializer_IsReturnedWhenRequested()
        {
            var resolver = new ModelSerializerResolver();
            resolver.Register<Customer>(MimeType.Js, typeof(StubSerializer<Customer>));
            var serializer = resolver.CreateSerializer<Customer>(MimeType.Js);
            Assert.IsNotNull(serializer);
            Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());
        }

        [Test]
        public void Register_OfSerializerForModel_DoesNotAffectOtherModel()
        {
            var resolver = new ModelSerializerResolver();
            resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
            var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
            Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());

            var serializer2 = resolver.CreateSerializer<Supplier>(MimeType.JSon);
            Assert.AreNotEqual(typeof(StubSerializer<Customer>), serializer2.GetType());
        }

        class Customer {}
        class Supplier { }

        class StubSerializer<T> : IModelSerializer<T>
        {
            public void Serialize(T model, string contentType, TextWriter writer, ModelMetadataProvider metadataProvider)
            {
                throw new System.NotImplementedException();
            }

            public T Deserialize(string prefix, string contentType, HttpRequestBase request, ModelMetadataProvider metadataProvider)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
