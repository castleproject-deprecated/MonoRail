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
	using System.IO;
	using App;
	using NUnit.Framework;
	using Runtime;

	/// <summary>
	/// This class tests strange Velocimacro issues.
	/// </summary>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	[TestFixture]
	public class VelocimacroTestCase
	{
		private String template1 = "#macro(fooz $a)$a#end#macro(bar $b)#fooz($b)#end#foreach($i in [1..3])#bar($i)#end";
		private String result1 = "123";

		public VelocimacroTestCase()
		{
			try
			{
				Velocity.SetProperty(RuntimeConstants.VM_PERM_INLINE_LOCAL, true);
				Velocity.Init();
			}
			catch(Exception)
			{
				throw new Exception("Cannot setup VelocimacroTestCase!");
			}
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		[Test]
		public virtual void Test_Run()
		{
			VelocityContext context = new VelocityContext();

			StringWriter writer = new StringWriter();
			Velocity.Evaluate(context, writer, "vm_chain1", template1);

			String output = writer.ToString();

			Assert.AreEqual(result1, output);
		}
	}
}