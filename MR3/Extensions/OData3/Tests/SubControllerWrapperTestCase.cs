using System;
using Castle.MonoRail.Hosting.Mvc.Typed;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	[TestFixture]
	public class SubControllerWrapperTestCase 
	{
		[Test]
		public void aaaaa()
		{
			var desc = new TypedControllerDescriptor(typeof(object));
			var controller = new object();



			var wrapper = new SubControllerWrapper(typeof(object), c => new TypedControllerPrototype(desc, controller));

			
		}
	}
}
