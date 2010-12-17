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

namespace Castle.MonoRail.ActiveRecordSupport.Scaffold.Helpers
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Text;
	using System.Reflection;
	using Castle.ActiveRecord.Framework.Internal;
	using Castle.MonoRail.Framework.Helpers;

	public class ARFormHelper : FormHelper
	{
		private StringBuilder stringBuilder = new StringBuilder(1024);

		private IDictionary model2nestedInstance = new Hashtable();
		
		private static readonly int[] Months = { 1,2,3,4,5,6,7,8,9,10,11,12 };
		private static readonly int[] Days = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 
		                                       11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 
		                                       21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
		private static readonly int[] Years;
		private TextHelper textHelper = new TextHelper();

		static ARFormHelper()
		{
			var lastYear = DateTime.Now.Year;

			Years = new int[lastYear - 1950 + 50];
			
			for(var year = 1950; year < lastYear + 50; year++)
			{
				Years[year - 1950] = year;
			}
		}

		public TextHelper TextHelper
		{
			get { return textHelper; }
		}

		public ICollection GetModelHierarchy(ActiveRecordModel model, object instance)
		{
			var list = new ArrayList();

			var hierarchy = model;

			while(hierarchy != null)
			{
				list.Add(hierarchy);

				hierarchy = ActiveRecordModel.GetModel(hierarchy.Type.BaseType);
			}

			hierarchy = model;

			while(hierarchy != null)
			{
				foreach(var nested in hierarchy.Components)
				{
					var nestedInstance = nested.Property.GetValue(instance, null);

					if (nestedInstance == null)
					{
						nestedInstance = CreationUtil.Create(nested.Property.PropertyType);
					}

					if (nestedInstance != null)
					{
						model2nestedInstance[nested.Model] = nestedInstance;
					}

					list.Add(nested.Model);
				}

				hierarchy = ActiveRecordModel.GetModel(hierarchy.Type.BaseType);
			}

			return list;
		}

		#region CanHandle methods

		public bool CanHandle(FieldModel field)
		{
			return CanHandleType(field.Field.FieldType);
		}

		public bool CanHandle(PropertyModel propModel)
		{
			return CanHandleType(propModel.Property.PropertyType);
		}

		public bool CanHandle(PropertyInfo propInfo)
		{
			return CanHandleType(propInfo.PropertyType);
		}

		public bool CanHandle(BelongsToModel model)
		{
			return CheckModelAndKeyAreAccessible(model.BelongsToAtt.Type);
		}

		public bool CanHandle(HasManyModel model)
		{
			if (!model.HasManyAtt.Inverse)
			{
				return CheckModelAndKeyAreAccessible(model.HasManyAtt.MapType);
			}
			return false;
		}

		public bool CanHandle(HasAndBelongsToManyModel model)
		{
			if (!model.HasManyAtt.Inverse)
			{
				return CheckModelAndKeyAreAccessible(model.HasManyAtt.MapType);
			}
			return false;
		}

		private bool CheckModelAndKeyAreAccessible(Type type)
		{
			var otherModel = ActiveRecordModel.GetModel(type);

			var keyModel = ObtainPKProperty(otherModel);

			if (otherModel == null || keyModel == null)
			{
				return false;
			}

			return true;
		}

		private PrimaryKeyModel ObtainPKProperty(ActiveRecordModel model)
		{
			if (model == null) return null;

			var curModel = model;

			while(curModel != null)
			{
				var keyModel = curModel.PrimaryKey;
				
				if (keyModel != null)
				{
					return keyModel;
				}

				curModel = curModel.Parent;
			}

			return null;
		}

		private bool CanHandleType(Type type)
		{
			return (type.IsPrimitive || 
					type == typeof(String) || 
			        type == typeof(Decimal) || 
			        type == typeof(Single) || 
			        type == typeof(Double) || 
			        type == typeof(Byte) || 
			        type == typeof(SByte) || 
			        type == typeof(bool) || 
			        type == typeof(Enum) || 
			        type == typeof(DateTime));
		}

		#endregion

		#region CreateControl methods

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            FieldModel fieldModel, object instance)
		{
			stringBuilder.Length = 0;

			var fieldInfo = fieldModel.Field;

			var propName = CreatePropName(model, prefix, fieldInfo.Name);

			if (fieldInfo.FieldType == typeof(DateTime))
			{
				stringBuilder.Append(LabelFor(propName + "day", TextHelper.PascalCaseToWord(fieldInfo.Name) + ": &nbsp;"));
			}
			else
			{
				stringBuilder.Append(LabelFor(propName, TextHelper.PascalCaseToWord(fieldInfo.Name) + ": &nbsp;"));
			}

			var propAtt = fieldModel.FieldAtt;

			RenderAppropriateControl(model, fieldInfo.FieldType, propName, null, null,
			                         propAtt.Unique, propAtt.NotNull, propAtt.ColumnType, propAtt.Length);

			return stringBuilder.ToString();
		}

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            PropertyModel propertyModel, object instance)
		{
			stringBuilder.Length = 0;

			var prop = propertyModel.Property;

			// Skip non standard properties
			if (!prop.CanWrite || !prop.CanRead) return String.Empty;

			// Skip indexers
			if (prop.GetIndexParameters().Length != 0) return String.Empty;

			var propName = CreatePropName(model, prefix, prop.Name);

			if (prop.PropertyType == typeof(DateTime))
			{
				stringBuilder.Append(LabelFor(propName + "day", TextHelper.PascalCaseToWord(prop.Name) + ": &nbsp;"));
			}
			else
			{
				stringBuilder.Append(LabelFor(propName, TextHelper.PascalCaseToWord(prop.Name) + ": &nbsp;"));
			}

			var propAtt = propertyModel.PropertyAtt;

			RenderAppropriateControl(model, prop.PropertyType, propName, prop, null,
			                         propAtt.Unique, propAtt.NotNull, propAtt.ColumnType, propAtt.Length);

			return stringBuilder.ToString();
		}

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            PropertyInfo prop, object instance)
		{
			stringBuilder.Length = 0;

			// Skip non standard properties
			if (!prop.CanWrite || !prop.CanRead) return String.Empty;

			// Skip indexers
			if (prop.GetIndexParameters().Length != 0) return String.Empty;

			var propName = CreatePropName(model, prefix, prop.Name);

			if (prop.PropertyType == typeof(DateTime))
			{
				stringBuilder.Append(LabelFor(propName + "day", TextHelper.PascalCaseToWord(prop.Name) + ": &nbsp;"));
			}
			else
			{
				stringBuilder.Append(LabelFor(propName, TextHelper.PascalCaseToWord(prop.Name) + ": &nbsp;"));
			}

			RenderAppropriateControl(model, prop.PropertyType,
			                         propName, prop, null, false, false, null, 0);

			return stringBuilder.ToString();
		}

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            BelongsToModel belongsToModel, object instance)
		{
			stringBuilder.Length = 0;

			var prop = belongsToModel.Property;
			
			prefix += "." + prop.Name;

			var otherModel = ActiveRecordModel.GetModel(belongsToModel.BelongsToAtt.Type);

			var keyModel = ObtainPKProperty(otherModel);

			if (otherModel == null || keyModel == null)
			{
				return "Model not found or PK not found";
			}

			var items = CommonOperationUtils.FindAll(otherModel.Type);

			var propName = CreatePropName(model, prefix, keyModel.Property.Name);

			stringBuilder.Append(LabelFor(propName, TextHelper.PascalCaseToWord(prop.Name) + ": &nbsp;"));
			
			IDictionary attrs = new HybridDictionary(true);
			
			attrs["value"] = keyModel.Property.Name;
			
			if (!belongsToModel.BelongsToAtt.NotNull)
			{
				attrs.Add("firstOption", "Empty");
				attrs.Add("firstOptionValue", "");
			}

			stringBuilder.Append(Select(propName, items, attrs));

			return stringBuilder.ToString();
		}

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            HasManyModel hasManyModel, object instance)
		{
			stringBuilder.Length = 0;

			var prop = hasManyModel.Property;
			
			prefix += "." + prop.Name;

			var otherModel = ActiveRecordModel.GetModel(hasManyModel.HasManyAtt.MapType);

			var keyModel = ObtainPKProperty(otherModel);

			if (otherModel == null || keyModel == null)
			{
				return "Model not found or PK not found";
			}

			var source = CommonOperationUtils.FindAll(otherModel.Type);

			stringBuilder.Append(prop.Name + ": &nbsp;");
			stringBuilder.Append("<br/>\r\n");
			
			IDictionary attrs = new HybridDictionary(true);
			
			attrs["value"] = keyModel.Property.Name;
			
			var list = CreateCheckboxList(prefix, source, attrs);
			
			foreach(var item in list)
			{
				stringBuilder.Append(list.Item());
				
				stringBuilder.Append(item.ToString());
				
				stringBuilder.Append("<br/>\r\n");
			}

			return stringBuilder.ToString();
		}

		public String CreateControl(ActiveRecordModel model, String prefix, 
		                            HasAndBelongsToManyModel hasAndBelongsModel, object instance)
		{
			stringBuilder.Length = 0;

			var prop = hasAndBelongsModel.Property;
			
			prefix += "." + prop.Name;

			var otherModel = ActiveRecordModel.GetModel(hasAndBelongsModel.HasManyAtt.MapType);

			var keyModel = ObtainPKProperty(otherModel);

			if (otherModel == null || keyModel == null)
			{
				return "Model not found or PK not found";
			}

			var source = CommonOperationUtils.FindAll(otherModel.Type);

			stringBuilder.Append(prop.Name + ": &nbsp;");
			stringBuilder.Append("<br/>\r\n");
			
			IDictionary attrs = new HybridDictionary(true);
			
			attrs["value"] = keyModel.Property.Name;
			
			var list = CreateCheckboxList(prefix, source, attrs);
			
			foreach(var item in list)
			{
				stringBuilder.Append(list.Item());
				
				stringBuilder.Append(item.ToString());
				
				stringBuilder.Append("<br/>\r\n");
			}

			return stringBuilder.ToString();
		}

		#endregion

		private void RenderAppropriateControl(ActiveRecordModel model,
		                                      Type propType, string propName, PropertyInfo property,
		                                      object value, bool unique, bool notNull, String columnType, int length)
		{
			IDictionary htmlAttributes = new Hashtable();
			
			if (propType == typeof(String))
			{
				if (String.Compare("stringclob", columnType, true) == 0)
				{
					stringBuilder.AppendFormat(TextArea(propName));
				}
				else
				{
					if (length > 0)
					{
						htmlAttributes["maxlength"] = length.ToString();
					}
					
					stringBuilder.AppendFormat(TextField(propName, htmlAttributes));
				}
			}
			else if (propType == typeof(Int16) || propType == typeof(Int32) || propType == typeof(Int64))
			{
				stringBuilder.AppendFormat(NumberField(propName, htmlAttributes));
			}
			else if (propType == typeof(Single) || propType == typeof(Double) || propType == typeof(Decimal))
			{
				stringBuilder.AppendFormat(NumberField(propName, htmlAttributes));
			}
			else if (propType == typeof(DateTime))
			{
				stringBuilder.AppendFormat(Select(propName + "month", Months, htmlAttributes));
				stringBuilder.AppendFormat(Select(propName + "day", Days, htmlAttributes));
				stringBuilder.AppendFormat(Select(propName + "year", Years, htmlAttributes));
			}
			else if (propType == typeof(bool))
			{
				stringBuilder.Append(CheckboxField(propName));
			}
			else if (propType == typeof(Enum))
			{
				// TODO: Support flags as well

				var names = Enum.GetNames(propType);

				IList options = new ArrayList();

				foreach(var name in names)
				{
					options.Add(String.Format("{0} {1}\r\n",
					                          RadioField(propName, name), LabelFor(name, TextHelper.PascalCaseToWord(name))));
				}
			}
		}

		private static string CreatePropName(ActiveRecordModel model, String prefix, String name)
		{
			string propName;

			if (model.IsNestedType)
			{
				propName = String.Format("{0}.{1}.{2}", prefix, model.ParentNested.Property.Name, name);
			}
			else
			{
				propName = String.Format("{0}.{1}", prefix, name);
			}

			return propName;
		}
	}
}
