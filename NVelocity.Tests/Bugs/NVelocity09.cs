using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVelocity.Tests.Bugs
{
	using System.IO;
	using App;
	using Commons.Collections;
	using NUnit.Framework;
	using Runtime;
	using Test;

	[TestFixture, Ignore("Won't fix.")]
	public class NVelocity09 : BaseTestCase
	{
		[Test]
		public void Test()
		{
			var velocityEngine = new VelocityEngine();

			ExtendedProperties extendedProperties = new ExtendedProperties();
			extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

			velocityEngine.Init(extendedProperties);

			VelocityContext context = new VelocityContext();

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "nv09", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new StringWriter();

			template.Merge(context, writer);
		}
	}
}
