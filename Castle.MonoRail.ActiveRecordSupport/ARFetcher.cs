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

namespace Castle.MonoRail.ActiveRecordSupport
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using NHibernate;
	using NHibernate.Criterion;

	using Castle.ActiveRecord;
	using Castle.ActiveRecord.Framework.Internal;
	using Castle.Components.Binder;
	using Castle.MonoRail.Framework;

	/// <summary>
	/// Class responsible on loading records for parameters marked with the <see cref="ARFetchAttribute" />.
	/// </summary>
	public class ARFetcher
	{
		private readonly IConverter converter;

		public ARFetcher(IConverter converter)
		{
			this.converter = converter;
		}

		public object FetchActiveRecord(ParameterInfo param, 
			ARFetchAttribute attr, 
			IRequest request, 
			IDictionary<string, object> customActionParameters)
		{
			var type = param.ParameterType;

			var isArray = type.IsArray;

			if (isArray) type = type.GetElementType();

			var model = ActiveRecordModel.GetModel(type);

			if (model == null)
			{
				throw new MonoRailException(String.Format("'{0}' is not an ActiveRecord " +
					"class. It could not be bound to an [ARFetch] attribute.", type.Name));
			}

			if (model.CompositeKey != null)
			{
				throw new MonoRailException("ARFetch only supports single-attribute primary keys");
			}

			var webParamName = attr.RequestParameterName ?? param.Name;

			if (!isArray)
			{
				var value = GetParameterValue(webParamName, customActionParameters, request);
				return LoadActiveRecord(type, value, attr, model);
			}

			var pks = GetParameterValues(webParamName, customActionParameters, request);

			var objs = Array.CreateInstance(type, pks.Length);

			for(var i = 0; i < objs.Length; i++)
			{
				objs.SetValue(LoadActiveRecord(type, pks[i], attr, model), i);
			}

			return objs;
		}

		private object[] GetParameterValues(string webParamName, IDictionary<string, object> customActionParameters, IRequest request)
		{
			object tmp;
			object[] pks;
			if(customActionParameters.TryGetValue(webParamName, out tmp) == false || (tmp is Array)==false)
				pks = request.Params.GetValues(webParamName);
			else
				pks = (object[]) tmp;

			if (pks == null)
			{
				pks = new object[0];
			}
			return pks;
		}

		private string GetParameterValue(string webParamName, IDictionary<string, object> customActionParameters, IRequest request)
		{
			string value;
			object tmp;
			if (customActionParameters.TryGetValue(webParamName, out tmp) == false || tmp == null)
				value = request.Params[webParamName];
			else
				value = tmp.ToString();
			return value;
		}

		private object LoadActiveRecord(Type type, object pk, ARFetchAttribute attr, ActiveRecordModel model)
		{
			object instance = null;

			if (pk != null && !String.Empty.Equals(pk))
			{
				var pkModel = ObtainPrimaryKey(model);

				var pkType = pkModel.Property.PropertyType;

				bool conversionSucceeded;
				var convertedPk = converter.Convert(pkType, pk.GetType(), pk, out conversionSucceeded);
				
				if (!conversionSucceeded)
				{
					throw new MonoRailException("ARFetcher could not convert PK {0} to type {1}", pk, pkType);
				}

				if (string.IsNullOrEmpty(attr.Eager))
				{
					// simple load
					instance = ActiveRecordMediator.FindByPrimaryKey(type, convertedPk, attr.Required);
				}
				else
				{
					// load using eager fetching of lazy collections
					var criteria = DetachedCriteria.For(type);
					criteria.Add(Expression.Eq(pkModel.Property.Name, convertedPk));
					foreach (var associationToEagerFetch in attr.Eager.Split(','))
					{
						var clean = associationToEagerFetch.Trim();
						if (clean.Length == 0)
						{
							continue;
						}
						
						criteria.SetFetchMode(clean, FetchMode.Eager);
					}
					
					var result = (object[]) ActiveRecordMediator.FindAll(type, criteria);
					if (result.Length > 0)
						instance = result[0];
				}
			}

			if (instance == null && attr.Create)
			{
				instance = Activator.CreateInstance(type);
			}

			return instance;
		}

		private static PrimaryKeyModel ObtainPrimaryKey(ActiveRecordModel model)
		{
			if (model.IsJoinedSubClass || model.IsDiscriminatorSubClass)
			{
				return ObtainPrimaryKey(model.Parent);
			}
			return model.PrimaryKey;
		}
 
	}
}
