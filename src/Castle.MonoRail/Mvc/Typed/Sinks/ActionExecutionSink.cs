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
#endregion

namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.Composition;
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
			var @params = executionCtx.HttpContext.Request.Params;
			var routeData = executionCtx.RouteData;
			var args = new List<object>();
			var paramName = String.Empty;
			object value = null;

			try
			{
				foreach (var parameterDescriptor in descriptor.Parameters)
				{
					paramName = parameterDescriptor.Name;

					value = routeData.Values[paramName] ?? @params[paramName];

					object result = null;

					var targetType = parameterDescriptor.Type;

					if (value != null && 
						targetType != typeof(string) && 
						targetType != typeof(object))
					{
						var converter = TypeDescriptor.GetConverter(targetType);

						// temporary. TypeConverter may have an awful perf. need to investigate
						if (converter.CanConvertFrom(value.GetType()))
						{
							result = converter.ConvertTo(value, targetType);
						}
						else
						{
							// what to do?
							throw new FormatException("Could not convert '" + value + "' to target type " + targetType);
						}
					}

					args.Add(result);
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
