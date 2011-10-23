// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
namespace NVelocity.Test
{
	using System;
	using System.Reflection;
	using global::Commons.Collections;
	using NUnit.Framework;
	using Runtime;
	using Util.Introspection;

	/// <summary>
	/// Test Velocity Introspector
	/// </summary>
	[TestFixture]
	public class IntrospectorTest
	{
		[Test]
		public void Test_Evaluate()
		{
			IRuntimeServices rs = RuntimeSingleton.RuntimeServices;
			Introspector i = new Introspector(rs);
			MethodInfo mi = i.GetMethod(typeof(VelocityTest), "Test_Evaluate", null);
			Assert.IsNotNull(mi, "Expected to find VelocityTest.Test_Evaluate");
			Assert.IsTrue(mi.ToString().Equals("Void Test_Evaluate()"), "method not found");

			mi = i.GetMethod(typeof(ExtendedProperties), "GetString", new Object[] {"parm1", "parm2"});
			Assert.IsNotNull(mi, "Expected to find ExtendedProperties.GetString(String, String)");
			Assert.IsTrue(mi.ToString().Equals("System.String GetString(System.String, System.String)"), "method not found");
		}
	}
}