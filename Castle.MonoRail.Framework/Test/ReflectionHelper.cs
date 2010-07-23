// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Reflection;

namespace Castle.MonoRail.TestSupport
{
	/// <summary>
	/// I think the class name says it all.
	/// </summary>
	public static class ReflectionHelper
	{
		/// <summary>
		/// Pedent
		/// </summary>
		/// <typeparam name="ObjectType"></typeparam>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static object RunInstanceMethod<ObjectType>(ObjectType objectInstance, string methodName)
		{
			return RunInstanceMethod(typeof(ObjectType), objectInstance, methodName);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static object RunInstanceMethod(Type objectType, object objectInstance, string methodName)
		{
			var methodParameters = new object[0];
			return RunInstanceMethod(objectType, objectInstance, methodName, ref methodParameters, BindingFlags.Default);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <typeparam name="ObjectType"></typeparam>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <returns></returns>
		public static object RunInstanceMethod<ObjectType>(ObjectType objectInstance, string methodName, ref object[] methodParameters)
		{
			return RunInstanceMethod(typeof(ObjectType), objectInstance, methodName, ref methodParameters);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <returns></returns>
		public static object RunInstanceMethod(Type objectType, object objectInstance, string methodName, ref object[] methodParameters)
		{
			return RunInstanceMethod(objectType, objectInstance, methodName, ref methodParameters, BindingFlags.Default);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <typeparam name="ObjectType"></typeparam>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <param name="extraFlags"></param>
		/// <returns></returns>
		public static object RunInstanceMethod<ObjectType>(ObjectType objectInstance, string methodName, ref object[] methodParameters, BindingFlags extraFlags)
		{
			return RunInstanceMethod(typeof(ObjectType), objectInstance, methodName, ref methodParameters, extraFlags);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="objectInstance"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <param name="extraFlags"></param>
		/// <returns></returns>
		public static object RunInstanceMethod(Type objectType, object objectInstance, string methodName, ref object[] methodParameters, BindingFlags extraFlags)
		{
			var eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | extraFlags;
			return RunMethod(objectType, objectInstance, methodName, ref methodParameters, eFlags);
		}

		private static object RunMethod(Type objectType, object objectInstance, string methodName, ref object[] methodParameters, BindingFlags bindingFlags)
		{
			try
			{
				var methodInfo = objectType.GetMethod(methodName, bindingFlags);

				if (methodInfo == null)
					throw new ArgumentException("There is no method '" + methodName + "' for type '" + objectType + "'.");

				var returnValue = methodInfo.Invoke(objectInstance, methodParameters);

				return returnValue;
			}
			catch (TargetInvocationException ex)
			{
				throw (ex.InnerException != null) ? ex.InnerException : ex;
			}
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static object RunStaticMethod(Type objectType, string methodName)
		{
			var methodParameters = new object[0];
			return RunStaticMethod(objectType, methodName, ref methodParameters, BindingFlags.Default);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <returns></returns>
		public static object RunStaticMethod(Type objectType, string methodName, ref object[] methodParameters)
		{
			return RunStaticMethod(objectType, methodName, ref methodParameters, BindingFlags.Default);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="methodName"></param>
		/// <param name="methodParameters"></param>
		/// <param name="extraFlags"></param>
		/// <returns></returns>
		public static object RunStaticMethod(Type objectType, string methodName, ref object[] methodParameters, BindingFlags extraFlags)
		{
			var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | extraFlags;
			return RunMethod(objectType, null, methodName, ref methodParameters, bindingFlags);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <typeparam name="ReturnType"></typeparam>
		/// <typeparam name="ObjectType"></typeparam>
		/// <param name="objectInstance"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static ReturnType GetField<ReturnType, ObjectType>(ObjectType objectInstance, string fieldName)
		{
			var fieldInfo = typeof(ObjectType).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return (ReturnType)fieldInfo.GetValue(objectInstance);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <typeparam name="ObjectType"></typeparam>
		/// <param name="objectInstance"></param>
		/// <param name="fieldName"></param>
		/// <param name="fieldValue"></param>
		public static void SetField<ObjectType>(ObjectType objectInstance, string fieldName, object fieldValue)
		{
			SetField(typeof(ObjectType), objectInstance, fieldName, fieldValue);
		}

		/// <summary>
		/// Pedent
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="objectInstance"></param>
		/// <param name="fieldName"></param>
		/// <param name="fieldValue"></param>
		public static void SetField(Type objectType, object objectInstance, string fieldName, object fieldValue)
		{
			var fieldInfo = objectType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			fieldInfo.SetValue(objectInstance, fieldValue);
		}
	}
}
