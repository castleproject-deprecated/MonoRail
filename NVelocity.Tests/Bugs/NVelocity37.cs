namespace NVelocity.Tests.Bugs
{
	using System;
	using System.IO;
	using App;
	using Commons.Collections;
	using NUnit.Framework;
	using Runtime;
	using Test;

	[TestFixture]
	public class NVelocity37 : BaseTestCase
	{
		[Test]
		public void Test()
		{
			var velocityEngine = new VelocityEngine();

			ExtendedProperties extendedProperties = new ExtendedProperties();
			extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

			velocityEngine.Init(extendedProperties);

			VelocityContext context = new VelocityContext();

			context.Put("yada", "line");

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "nv37", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new StringWriter();

#pragma warning disable 612,618
			velocityEngine.MergeTemplate("nv37.vm", context, writer);
#pragma warning restore 612,618

			//template.Merge(context, writer);

			Console.WriteLine(writer);
		}
	}
}
