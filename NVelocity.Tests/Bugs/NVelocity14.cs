namespace NVelocity.Tests.Bugs
{
	using System;
	using System.IO;
	using App;
	using Commons.Collections;
	using Exception;
	using NUnit.Framework;
	using Runtime;
	using Test;

	[TestFixture]
	public class NVelocity14 : BaseTestCase
	{
		[Test, ExpectedException(typeof(ParseErrorException))]
		public void Test()
		{
			var velocityEngine = new VelocityEngine();

			ExtendedProperties extendedProperties = new ExtendedProperties();
			extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

			velocityEngine.Init(extendedProperties);

			VelocityContext context = new VelocityContext();

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "nv14", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new StringWriter();

			template.Merge(context, writer);

			Console.WriteLine(writer);
		}
	}
}
