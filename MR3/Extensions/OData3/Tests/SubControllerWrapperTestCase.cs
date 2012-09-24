using System;
using Castle.MonoRail.Hosting.Mvc.Typed;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	using FluentAssertions;
	using Microsoft.Data.Edm.Library;

	[TestFixture]
	public class SubControllerWrapperTestCase 
	{
		[Test]
		public void controllerWithNoODataOps_generates_no_functions()
		{
			var edm = Models.ModelWithAssociation.Build();
			var desc = new TypedControllerDescriptorBuilder().Build(typeof(object));
			var controller = new object();
			var wrapper = new SubControllerWrapper(typeof(object), c => new TypedControllerPrototype(desc, controller));

			var functions = wrapper.GetFunctionImports(edm);
			functions.Should().NotBeNull();
			functions.Should().HaveCount(0);
		}
	}
}
