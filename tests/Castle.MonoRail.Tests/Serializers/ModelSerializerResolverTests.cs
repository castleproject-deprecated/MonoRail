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
            public void Serialize(T model, string contentType, TextWriter writer)
            {
                throw new System.NotImplementedException();
            }

            public T Deserialize(string prefix, string contentType, HttpRequestBase request)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
