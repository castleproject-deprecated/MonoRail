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
namespace Castle.MonoRail.Mvc.Typed.Sinks
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web;
	using Castle.Components.Binder;

	[Export(typeof(IActionExecutionSink))]
	public class ActionExecutionSink : BaseControllerExecutionSink, IActionExecutionSink
	{
		public ActionExecutionSink()
		{
			DataBinder = new DataBinder();
		}

		protected DataBinder DataBinder { get; set; }

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
			var paramName = String.Empty;
			var value = String.Empty;

			var @params = executionCtx.HttpContext.Request.Params;
			var args = new List<object>();

			try
			{
				foreach (var param in descriptor.Parameters.Values)
				{
					paramName = param.Name;

					if (@params.AllKeys.Any(key => key == paramName))
					{
						value = @params[paramName];

						TryConvert(param, value, args);
						continue;
					}

					if (executionCtx.RouteData.Values.ContainsKey(paramName))
					{
						value = (string) executionCtx.RouteData.Values[paramName];

						TryConvert(param, value, args);
						continue;
					}

					if (typeof(HttpContextBase).IsAssignableFrom(param.Type))
					{
						args.Add(executionCtx.HttpContext);
						continue;
					}

					if (typeof(ControllerContext).IsAssignableFrom(param.Type))
					{
						args.Add(executionCtx.ControllerContext);
						continue;
					}

					args.Add(null);
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

		private void TryConvert(ParameterDescriptor param, string value, List<object> args)
		{
			bool succeeded;
			var converted = DataBinder.Converter.Convert(param.Type, typeof(string), value, out succeeded);
			args.Add(converted);
		}

		private object PerformSimpleExecution(ControllerExecutionContext executionCtx, ActionDescriptor descriptor)
		{
			return descriptor.Action(executionCtx.Controller, new object[0]);
		}
	}
}
