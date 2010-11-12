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
namespace Castle.MonoRail3.Hosting.Mvc.Typed
{
	using System.ComponentModel.Composition;
	using ControllerExecutionSink;
	using Primitives.Mvc;

	[Export(typeof(IActionResultSink))]
	public class ActionResultExecutionSink : BaseControllerExecutionSink, IActionResultSink
	{
		[Import]
		public IMonoRailServices Services { get; set; }

		public override void Invoke(ControllerExecutionContext executionCtx)
		{
			var result = executionCtx.InvocationResult as ActionResult;

			if (result != null)
			{
				var areaName = executionCtx.ControllerDescriptor.Area;
				var controllerName = executionCtx.ControllerDescriptor.Name;
				var actionName = executionCtx.SelectedAction.Name;

				var resultCtx = new ActionResultContext(areaName, controllerName, actionName, executionCtx.HttpContext);

				result.Execute(resultCtx, Services);
			}

			Proceed(executionCtx);
		}
	}
}
