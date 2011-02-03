// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Helpers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using Castle.MonoRail.Framework;

	public class ForHelper : AbstractFormRelatedHelper
	{
		public string Display(string target)
		{
			var formHelper = (FormHelper) ControllerContext.Helpers["FormHelper"];

			return RenderHtml(target, "DisplayTemplates") ?? formHelper.LiteralFor(target);
		}

		public string Editor(string target)
		{
			var formHelper = (FormHelper) ControllerContext.Helpers["FormHelper"];

			return RenderHtml(target, "EditorTemplates") ?? formHelper.TextField(target);
		}

		private string RenderHtml(string target, string folderName)
		{
			string returnValue = null;
			var meta = new ModelMetadata();

			foreach (var template in GetTemplate(target, folderName,
												 propertyInfo =>
												 {
													 meta.DisplayName = propertyInfo.Name;
													 meta.Value = ObtainValue(RequestContext.All, target).ToString();
													 meta.Target = target;
												 }))
			{
				using (var writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					var context = new ControllerContext
					{
						Helpers = ControllerContext.Helpers
					};
					context.PropertyBag["model"] = meta;
					Context.Services.ViewEngineManager.ProcessPartial(template, writer, Context, Controller, context);
					returnValue = writer.ToString();
					break;
				}
			}
			return returnValue;
		}

		private IEnumerable<string> GetTemplatePaths(string folderName)
		{
			yield return Path.Combine(ControllerContext.ViewFolder, folderName);

			yield return String.Format("shared/{0}", folderName);
		}

		private IEnumerable<string> GetTemplate(string target, string folderName, Action<PropertyInfo> action)
		{
			var targetProperty = ObtainTargetProperty(RequestContext.All, target, action);
			var templateName = targetProperty.Name;

			foreach (var templatePath in GetTemplatePaths(folderName))
			{
				var fullPath = Path.Combine(templatePath, templateName);
				if (Context.Services.ViewEngineManager.HasTemplate(fullPath))
				{
					yield return fullPath;
				}
			}

			var fieldType = Nullable.GetUnderlyingType(targetProperty.PropertyType) ?? targetProperty.PropertyType;
			templateName = fieldType.Name;

			foreach (var templatePath in GetTemplatePaths(folderName))
			{
				var fullPath = Path.Combine(templatePath, templateName);
				if (Context.Services.ViewEngineManager.HasTemplate(fullPath))
				{
					yield return fullPath;
				}
			}

			if (fieldType == typeof(string))
			{
				yield return "String";
			}
			else if (fieldType.IsInterface)
			{
				if (typeof(IEnumerable).IsAssignableFrom(fieldType))
				{
					yield return "Collection";
				}

				yield return "Object";
			}
			else
			{
				bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(fieldType);

				while (true)
				{
					fieldType = fieldType.BaseType;
					if (fieldType == null)
						break;

					if (isEnumerable && fieldType == typeof(Object))
					{
						yield return "Collection";
					}

					yield return fieldType.Name;
				}
			}
		}
	}

	public class ModelMetadata
	{
		public string Value { get; set; }
		public string DisplayName { get; set; }
		public string Target { get; set; }
	}
}
