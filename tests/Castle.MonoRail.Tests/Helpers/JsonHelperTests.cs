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
            _jsonHelper = new JsonHelper(_ctx);
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
