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
	/// This class is intended to test the App.Velocity class.
	/// </summary>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <author> <a href="mailto:jon@latchkey.com">Jon S. Stevens</a>
	/// </author>
	[TestFixture]
	public class VelocityAppTestCase : BaseTestCase
	{
		private StringWriter compare1 = new StringWriter();
		private String input1 = "My name is $name -> $Floog";
		private String result1 = "My name is jason -> floogie woogie";

		public VelocityAppTestCase()
		{
			try
			{
				Velocity.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);
				Velocity.Init();
			}
			catch(Exception e)
			{
				throw new Exception("Cannot setup VelocityAppTestCase!", e);
			}
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		[Test]
		public virtual void Test_Run()
		{
			VelocityContext context = new VelocityContext();
			context.Put("name", "jason");
			context.Put("Floog", "floogie woogie");

			Velocity.Evaluate(context, compare1, "evaltest", input1);

			/*
	    FIXME: Not tested right now.

	    StringWriter result2 = new StringWriter();
	    Velocity.mergeTemplate("mergethis.vm",  context, result2);

	    StringWriter result3 = new StringWriter();
	    Velocity.invokeVelocimacro("floog", "test", new String[2], 
	    context, result3);*/

			if (!result1.Equals(compare1.ToString()))
			{
				Assert.Fail("Output incorrect.");
			}
		}
	}
}