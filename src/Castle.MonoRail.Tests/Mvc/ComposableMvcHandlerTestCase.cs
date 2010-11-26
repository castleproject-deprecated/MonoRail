//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
namespace Castle.MonoRail.Tests.Mvc
{
	using System.ComponentModel.Composition.Hosting;
	using System.Web;
	using System.Web.Routing;
	using Castle.MonoRail.Internal;
	using Castle.MonoRail.Mvc;
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
