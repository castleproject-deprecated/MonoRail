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
namespace Castle.MonoRail.Mvc.Typed.Sinks
{
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web;
	using Components.Binder;

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
			var args = new List<object>();
			

			foreach (var param in descriptor.Parameters.Values)
			{
				if (!param.DemandsCustomDataBinding)
				{
					DoDefaultDataBind(param, args, executionCtx);					
				}
				else
				{
					DoCustomDataBind(param, args, executionCtx);
				}
			}

			return descriptor.Action(executionCtx.Controller, args.ToArray());
		}

		private void DoCustomDataBind(ParameterDescriptor param, List<object> args, ControllerExecutionContext executionCtx)
		{
			args.Add(param.CustomBinder.Bind(executionCtx.HttpContext, param));
		}

		private void DoDefaultDataBind(ParameterDescriptor param, List<object> args, ControllerExecutionContext executionCtx)
		{
			var requestParams = executionCtx.HttpContext.Request.Params;

			string value;

			if (requestParams.AllKeys.Any(key => key == param.Name))
			{
				value = requestParams[param.Name];

				TryConvert(param, value, args);
				return;
			}

			if (executionCtx.RouteData.Values.ContainsKey(param.Name))
			{
				value = (string) executionCtx.RouteData.Values[param.Name];

				TryConvert(param, value, args);
				return;
			}

			if (typeof(HttpContextBase).IsAssignableFrom(param.Type))
			{
				args.Add(executionCtx.HttpContext);
				return;
			}

			if (typeof(ControllerContext).IsAssignableFrom(param.Type))
			{
				args.Add(executionCtx.ControllerContext);
				return;
			}

			args.Add(null);
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
