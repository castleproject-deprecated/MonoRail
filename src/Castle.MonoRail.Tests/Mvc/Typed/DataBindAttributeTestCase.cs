#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.MonoRail.Tests.Mvc.Typed
{
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
