#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Tests.Mvc
{
	using System.ComponentModel.Composition.Hosting;
	using System.Web;
	using System.Web.Routing;
	using Internal;
	using MonoRail.Mvc;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ComposableMvcHandlerTestCase
	{
		[Test]
		public void ProcessRequest_should_invoke_pipeline_runner()
		{
			var routeData = new RouteData();
			var parser = new Mock<RequestParser>();
			var pipeline = new Mock<PipelineRunner>();
			var context = new Mock<HttpContextBase>();

			var handler = new ComposableMvcHandler{RequestParser = parser.Object, Runner = pipeline.Object};

			parser.Setup(p => p.ParseDescriminators(It.IsAny<HttpRequestBase>())).Returns(routeData);

			pipeline.Setup(p => p.Process(routeData, context.Object));

			context.SetupGet(ctx => ctx.Items[ContainerManager.RequestContainerKey]).Returns(new CompositionContainer());

			handler.ProcessRequest(context.Object);

			pipeline.VerifyAll();
		}
	}
}
