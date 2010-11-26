#region License
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
#endregion
namespace Castle.MonoRail.WindsorIntegration
{
	using System.Collections;
	using System.ComponentModel.Composition;
	using System.Web;
	using System.Web.Routing;
	using Mvc;
	using Mvc.Typed;
	using Primitives.Mvc;
	using Windsor;

	[Export(typeof(ControllerProvider))]
	[ExportMetadata("Order", 1000)]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class WindsorControllerProvider : ControllerProvider
	{
		private readonly HttpContextBase httpContext;
		private readonly HttpRequestBase request;
		private readonly HttpResponseBase response;
		private readonly ControllerContext controllerContext;

		[ImportingConstructor]
		public WindsorControllerProvider(HttpContextBase httpContext, HttpRequestBase request, HttpResponseBase response, ControllerContext controllerContext)
		{
			this.httpContext = httpContext;
			this.request = request;
			this.response = response;
			this.controllerContext = controllerContext;
		}

		[Import]
		public ControllerDescriptorBuilder DescriptorBuilder { get; set; }

		public override ControllerMeta Create(RouteData data)
		{
			TypedControllerMeta meta = null;

			var accessor = httpContext.ApplicationInstance as IContainerAccessor;
			if (accessor != null)
			{
				var container = accessor.Container;
				var controllerName = data.GetRequiredString("controller").ToLowerInvariant();

				if (!container.Kernel.HasComponent(controllerName)) return null;

				var args = CreateArgs();
				var controller = container.Resolve<object>(controllerName, args);

				var descriptor = DescriptorBuilder.Build(controller.GetType());

				meta = new TypedControllerMeta(controller, descriptor);
			}

			return meta;
		}

		private IDictionary CreateArgs()
		{
			var hashtable = new Hashtable();
			hashtable["httpContext"] = httpContext;
			hashtable["request"] = request;
			hashtable["response"] = response;
			hashtable["controllerContext"] = controllerContext;
			return hashtable;
		}
	}
}