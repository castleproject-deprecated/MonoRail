namespace Castle.MonoRail.Tests.Serializers.Form
{
    using System.Collections.Generic;
    using Castle.MonoRail.Serialization;
    using NUnit.Framework;

    [TestFixture]
    public class FormBasedSerializerTests
    {
        [Test]
        public void Deserialize_WhenEmptyInput_JustInstantiateModel()
        {
            var ctx = new HttpContextStub();
            
            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));
            
            Assert.IsNotNull(model);
        }

        [Test]
        public void Deserialize_WithDepth0StringInput_FillsProperty()
        {
            var ctx = new HttpContextStub();
            var form = ctx.RequestStub.Form;
            form["customer[name]"] = "hammett";

            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));

            Assert.AreEqual("hammett", model.Name);
        }

        [Test]
        public void Deserialize_WithDepth0Int32Input_FillsProperty()
        {
            var ctx = new HttpContextStub();
            var form = ctx.RequestStub.Form;
            form["customer[age]"] = "32";

            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));

            Assert.AreEqual(32, model.Age);
        }

        [Test]
        public void Deserialize_WithDepth1StringInput_FillsProperty()
        {
            var ctx = new HttpContextStub();
            var form = ctx.RequestStub.Form;
            form["customer[address][city]"] = "kirkland";

            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));

            Assert.IsNotNull(model.Address);
            Assert.AreEqual("kirkland", model.Address.City);
        }

        [Test]
        public void Deserialize_WithDepth1IntForCollection_CreatesAndFillsCollection()
        {
            var ctx = new HttpContextStub();
            var form = ctx.RequestStub.Form;
            form["customer[permissions][id]"] = "10";

            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));

            Assert.IsNotNull(model.Permissions);
            Assert.AreEqual(1, model.Permissions.Count);
            Assert.AreEqual(10, model.Permissions[0].Id);
        }

        [Test]
        public void Deserialize_WithMultipleEntriesForSameCollection_CreatesAndFillsCollection()
        {
            var ctx = new HttpContextStub();
            var form = ctx.RequestStub.Form;
            form["customer[permissions][id]"] = "10,11,12,13";

            var serializer = new FormBasedSerializer<Customer>() as IModelSerializer<Customer>;
            var model = serializer.Deserialize("customer", "", ctx.Request, new StubModelMetadataProvider(null));

            Assert.IsNotNull(model.Permissions);
            Assert.AreEqual(4, model.Permissions.Count);
            Assert.AreEqual(10, model.Permissions[0].Id);
            Assert.AreEqual(11, model.Permissions[1].Id);
            Assert.AreEqual(12, model.Permissions[2].Id);
            Assert.AreEqual(13, model.Permissions[3].Id);
        }

        class Address
        {
            public string City { get; set; }
        }

        class Permission
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        class Customer
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Address Address { get; set; }
            public List<Permission> Permissions { get; set; }
        }
    }
}
