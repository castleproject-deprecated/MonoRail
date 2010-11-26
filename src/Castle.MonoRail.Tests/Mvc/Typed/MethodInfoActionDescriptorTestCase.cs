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
	using System;
	using MonoRail.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class MethodInfoActionDescriptorTestCase
	{
		private string _a;
		private int _b;

		[Test]
		public void Should_build_reusable_lambda_function_for_method()
		{
			var descriptor = new MethodInfoActionDescriptor(GetType().GetMethod("LambdaTarget"));

			Assert.AreEqual("LambdaTarget", descriptor.Name);

			var lambdaContainer = new MethodInfoActionDescriptorTestCase();

			var date = (DateTime) descriptor.Action(lambdaContainer, new object[] { "string", 2 });

			Assert.AreEqual(DateTime.Now.Date, date);
			Assert.AreEqual("string", lambdaContainer._a);
			Assert.AreEqual(2, lambdaContainer._b);
		}

//		[Test]
//		public void Should_build_a_map_of_the_parameters()
//		{
//			var descriptor = new MethodInfoActionDescriptor(GetType().GetMethod("LambdaTarget"));
//
//			Assert.IsFalse(descriptor.IsParameterLess);
//			Assert.AreEqual(2, descriptor.Parameters.Count);
//			Assert.AreEqual(typeof(string), descriptor.Parameters["a"].Type);
//			Assert.AreEqual(typeof(int), descriptor.Parameters["b"].Type);
//		}

		[Test]
		public void Should_detect_parameters_annoted_with_IActionParameterBinder()
		{
			var descriptor = new MethodInfoActionDescriptor(GetType().GetMethod("WithDatabinder"));

			Assert.IsFalse(descriptor.IsParameterLess);
			Assert.AreEqual(1, descriptor.Parameters.Count);

			var param = descriptor.Parameters["user"];

			Assert.AreEqual(typeof(User), param.Type);
			Assert.IsTrue(param.DemandsCustomDataBinding);
			Assert.IsAssignableFrom<DataBindAttribute>(param.CustomBinder);
		}

		public DateTime LambdaTarget(string a, int b)
		{
			_a = a;
			_b = b;

			return DateTime.Now.Date;
		}

		public object WithDatabinder([DataBind] User user)
		{
			return user;
		}
	}
}
