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

	[Export(typeof(IActionExecutionSink))]
	public class ActionExecutionSink : BaseControllerExecutionSink, IActionExecutionSink
	{
		public override void Invoke(ControllerExecutionContext executionCtx)
		{
			var result = executionCtx.SelectedAction.Action(executionCtx.Controller, new object[0]);

			executionCtx.InvocationResult = result;

			Proceed(executionCtx);
		}
	}
}
