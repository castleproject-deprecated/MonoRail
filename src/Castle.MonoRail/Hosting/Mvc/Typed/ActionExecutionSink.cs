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
namespace Castle.MonoRail.Hosting.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;

	[Export(typeof(IActionExecutionSink))]
	public class ActionExecutionSink : BaseControllerExecutionSink, IActionExecutionSink
	{
		public override void Invoke(ControllerExecutionContext executionCtx)
		{
			var descriptor = executionCtx.SelectedAction;

			object result;
			
			if (descriptor.IsParameterLess)
				result = PerformSimpleExecution(executionCtx, descriptor);
			else
				result = PerformDataBindedExecution(executionCtx, descriptor);

			executionCtx.InvocationResult = result;

			Proceed(executionCtx);
		}

		private object PerformDataBindedExecution(ControllerExecutionContext executionCtx, ActionDescriptor descriptor)
		{
			var @params = executionCtx.HttpContext.Request.Params;
			var args = new List<object>();
			var paramName = String.Empty;
			var value = String.Empty;

			try
			{
				foreach (var param in descriptor.Parameters.Values)
				{
					paramName = param.Name;

					if (@params.AllKeys.Any(key => key == paramName))
					{
						value = @params[paramName];

						args.Add(value);
					}
					else
					{
						args.Add(null);
					}
				}
			}
			catch (FormatException ex)
			{
				throw new Exception(
					String.Format("Could not convert {0} to request type. " +
								  "Argument value is '{1}'", paramName, value), ex);
			}
			catch (Exception ex)
			{
				throw new Exception(
					String.Format("Error building method arguments. " +
								  "Last param analyzed was {0} with value '{1}'", paramName, value), ex);
			}

			return descriptor.Action(executionCtx.Controller, args.ToArray());
		}

		private object PerformSimpleExecution(ControllerExecutionContext executionCtx, ActionDescriptor descriptor)
		{
			return descriptor.Action(executionCtx.Controller, new object[0]);
		}
	}
}
