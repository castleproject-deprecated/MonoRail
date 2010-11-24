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
namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;

	public class MethodInfoActionDescriptor : ActionDescriptor
	{
		private readonly MethodInfo method;

		public MethodInfoActionDescriptor(MethodInfo method)
		{
			this.method = method;

			InspectMethodInfo();

			Action = BuildInvocationFunc();
		}

		private void InspectMethodInfo()
		{
			Name = method.Name;

			foreach (var parameter in method.GetParameters())
			{
				var descriptor = new ParameterDescriptor(parameter.Name, parameter.ParameterType);

				Parameters.Add(descriptor.Name, descriptor);
			}
		}

		private Func<object, object[], object> BuildInvocationFunc()
		{
			// ((TController) c)._method( (TP0) p0, (TP1) p1, ..., (TPN) pN );

			var controllerType = method.DeclaringType;
			var target = Expression.Parameter(typeof(object), "target");
			var args = Expression.Parameter(typeof(object[]), "args");
			var expParams = BuildCastExpressionForParameters(args);
			var call = Expression.Call(Expression.Convert(target, controllerType), method, expParams);

			Expression lambdaBody;

			if (method.ReturnType != typeof(void))
			{
				lambdaBody = Expression.Convert(call, typeof(object));
			}
			else
			{
				var nullReturn = Expression.Constant(null, typeof(object));
				lambdaBody = Expression.Block(call, nullReturn);
			}

			var lambda = Expression.Lambda<Func<object, object[], object>>(lambdaBody, target, args);

			return lambda.Compile();
		}

		private IEnumerable<Expression> BuildCastExpressionForParameters(ParameterExpression args)
		{
			var parameters = new List<Expression>();
			var index = 0;

			foreach (var parameter in Parameters.Values)
			{
				var argAccess = Expression.ArrayAccess(args, Expression.Constant(index++));
				var exp = Expression.Convert(argAccess, parameter.Type);
				parameters.Add(exp);
			}

			return parameters;
		}
	}
}
