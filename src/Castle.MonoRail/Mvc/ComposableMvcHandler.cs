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
namespace Castle.MonoRail.Mvc
{
	using System;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.Web;
	using System.Web.Routing;
	using Internal;
	using Primitives;

	// does this need hardening?
	[Export(typeof(IComposableHandler))]
	public class ComposableMvcHandler : ComposableHandler
	{
		[Import]
		public RequestParser RequestParser { get; set; }

		[Import]
		public PipelineRunner Runner { get; set; }

		// no state changes
		// what exceptions we should guard against?
		public override void ProcessRequest(HttpContextBase context)
		{
			var data = RequestParser.ParseDescriminators(context.Request);

			var container = context.GetContainer();
			if (container == null) throw new InvalidOperationException("No request container available?");

			var batch = new CompositionBatch();
			batch.AddExportedValue(typeof(RouteData).GetContract(), data);
			batch.AddExportedValue(typeof(ControllerContext).GetContract(), new ControllerContext());
			container.Compose(batch);

			Runner.Process(data, context);
		}
	}
}
