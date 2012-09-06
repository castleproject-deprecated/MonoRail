namespace Castle.MonoRail.Extension.OData3.Tests.Serialization
{
    using MonoRail.OData.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ODataStackPayloadSerializerTestCase
    {
        private ODataStackPayloadSerializer _serializer;

        [SetUp]
        public void Init()
        {
            _serializer = new ODataStackPayloadSerializer(null, null);
        }

        [Test]
        public void a()
        {
            // _serializer.SerializeMany(model);
        }

    }
}
