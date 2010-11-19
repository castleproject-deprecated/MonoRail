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
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;
	using Primitives.Mvc;

	public class MethodInfoActionDescriptor : ActionDescriptor
	{
		private readonly MethodInfo _method;

		public MethodInfoActionDescriptor(MethodInfo method)
		{
			_method = method;

			Name = _method.Name;
			Action = BuildInvocationFunc();
		}

		private Func<object, object[], object> BuildInvocationFunc()
		{
			// ((TController) c)._method( (TP0) p0, (TP1) p1, ..., (TPN) pN );

			var controllerType = _method.DeclaringType;
			var target = Expression.Parameter(typeof(object), "target");
			var args = Expression.Parameter(typeof(object[]), "args");
			var expParams = BuildCastExpressionForParameters(args);
			var call = Expression.Call(Expression.Convert(target, controllerType), _method, expParams);

			Expression lambdaBody;

			if (_method.ReturnType != typeof(void))
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

			foreach (var parameter in _method.GetParameters())
			{
				var paramType = parameter.ParameterType;
				var argAccess = Expression.ArrayAccess(args, Expression.Constant(index++));
				var exp = Expression.Convert(argAccess, paramType);
				parameters.Add(exp);
			}

			return parameters;
		}
	}
}
