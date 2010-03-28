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

namespace Castle.MonoRail.Views.AspView.Compiler.PreCompilationSteps
{
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class ViewComponentTagsStep : IPreCompilationStep
	{
		int lastViewComponentsCounter;

		readonly ScriptToCodeTransformer scriptTransformer = new ScriptToCodeTransformer();

		public void Process(SourceFile file)
		{
			lastViewComponentsCounter = 0;
			file.RenderBody = Process(file.RenderBody, file);
		}

		private string Process(string currentContent, SourceFile file)
		{
			return Internal.RegularExpressions.ViewComponentTags.Replace(currentContent, delegate(Match match)
			{
				var componentTypeName = match.Groups["componentName"].Value;
				var attributes = match.Groups["attributes"].Value;
				var content = match.Groups["content"].Value;

				var viewComponentsCounter = lastViewComponentsCounter++;
				var parametersString = Utilities.GetAttributesStringFrom(attributes);

				var componentName = (componentTypeName + viewComponentsCounter).Replace('.', '_');

				string bodyHandlerName = null;
				var sectionHandlersArray = "null";

				var pairs = new List<string>();

				content = Internal.RegularExpressions.ViewComponentSectionTags.Replace(content, delegate(Match sectionTag)
				{
					var sectionName = sectionTag.Groups["sectionName"].Value;
					var sectionContent = sectionTag.Groups["content"].Value;
					var handlerName = componentName + "_" + sectionName;
					RegisterSectionHandler(handlerName, sectionContent, file);
					pairs.Add(string.Format(@"new KeyValuePair<string, ViewComponentSectionRendereDelegate>(""{0}"", {1}) ",
						sectionName, handlerName));
					return string.Empty;
				});

				if (content.Trim().Length > 0)
					bodyHandlerName = componentName + "_body";
				if (bodyHandlerName != null)
					RegisterSectionHandler(bodyHandlerName, content, file);

				if (pairs.Count > 0)
					sectionHandlersArray = string.Format(
						@"new KeyValuePair<string, ViewComponentSectionRendereDelegate>[] {{ {0} }}",
						string.Join(", ", pairs.ToArray()));

				return string.Format(
					@"<% InvokeViewComponent(""{0}""{1}, {2}, {3}); %>",
					componentTypeName, 
					parametersString,
					bodyHandlerName ?? "null", 
					sectionHandlersArray);
			});
		}

		private void RegisterSectionHandler(string handlerName, string sectionContent, SourceFile file)
		{
			var processedSection = sectionContent;
			if (Internal.RegularExpressions.ViewComponentTags.IsMatch(sectionContent))
				processedSection = Process(sectionContent, file);
			processedSection = scriptTransformer.Transform(processedSection);
			file.ViewComponentSectionHandlers[handlerName] = processedSection;
		}
	}
}
