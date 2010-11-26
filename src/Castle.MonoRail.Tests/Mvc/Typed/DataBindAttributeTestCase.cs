namespace Castle.MonoRail.Tests.Mvc.Typed
{
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using MonoRail.Mvc.Typed;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class DataBindAttributeTestCase
	{
		[Test]
		public void Bind_should_invoke_internal_DataBinder_using_values_from_Request_Params()
		{
			var http = new Mock<HttpContextBase>();
			var attr = new DataBindAttribute();
			var descriptor = new ParameterDescriptor("user", typeof (User));

			http.SetupGet(ctx => ctx.Request.Params).Returns(new NameValueCollection{{"user.Name", "Lyle"}});

			var result = attr.Bind(http.Object, descriptor);

			Assert.IsAssignableFrom<User>(result);
			Assert.AreEqual("Lyle", ((User) result).Name);
		}
	}
}
