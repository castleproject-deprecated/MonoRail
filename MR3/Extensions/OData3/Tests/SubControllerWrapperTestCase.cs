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
//		[Test]
//		public void controllerWithNoODataOps_generates_no_functions()
//		{
//			var edm = Models.ModelWithAssociation.Build();
//			var desc = new TypedControllerDescriptorBuilder().Build(typeof(ProductWithNoOpsController));
//			var controller = new object();
//			var wrapper = new SubControllerWrapper(
//				typeof(Models.ModelWithAssociation.Product), 
//				c => new TypedControllerPrototype(desc, controller)
//				, null);
//
//			var functions = wrapper.GetFunctionImports(edm);
//			functions.Should().NotBeNull();
//			functions.Should().HaveCount(0);
//		}
//
//		[Test]
//		public void controllerWithNoSingleOp_generates_1_function()
//		{
//			var edm = Models.ModelWithAssociation.Build();
//			var desc = new TypedControllerDescriptorBuilder().Build(typeof(ProductWithSingleOpController));
//			var controller = new object();
//			var wrapper = new SubControllerWrapper(
//				typeof(Models.ModelWithAssociation.Product), 
//				c => new TypedControllerPrototype(desc, controller)
//				, null);
//
//			var functions = wrapper.GetFunctionImports(edm);
//			functions.Should().NotBeNull();
//			functions.Should().HaveCount(1);
//		}

		class ProductWithNoOpsController : IODataEntitySubController<Models.ModelWithAssociation.Product>
		{
		}

		class ProductWithSingleOpController : IODataEntitySubController<Models.ModelWithAssociation.Product>
		{
			[ODataOperation]
			public void Publish()
			{
			}
		}
	}
}
