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
    using System;
    using System.Text.RegularExpressions;

	public class ProcessPropertiesStep : IPreCompilationStep
	{
		public void Process(SourceFile file)
		{
			file.RenderBody = Internal.RegularExpressions.PropertiesSection.Replace(file.RenderBody, delegate(Match match)
			{
				HandlePropertiesSection(match, file);
				return string.Empty;
			});
			file.RenderBody = Internal.RegularExpressions.PropertiesServerScriptSection.Replace(file.RenderBody, delegate(Match match)
			{
				HandlePropertiesSection(match, file);
				return string.Empty;
			});
		}

		private static void HandlePropertiesSection(Match match, SourceFile file)
		{
			var propertiesSection = match.Groups["properties"].Value.Trim();
			if (propertiesSection.EndsWith("%>"))
				propertiesSection = propertiesSection.Substring(0, propertiesSection.Length - 2);
			var propertiesDeclerations = propertiesSection.Split(';');
			foreach (var propertiesDecleration in propertiesDeclerations)
			{
				var prop = propertiesDecleration.Trim();
				if (prop == string.Empty)
					continue;
				var mainParts = prop.Split(new char[1] { '=' }, 2);
				var propDecleration = mainParts[0].Trim();
				string defaultValue = null;
				if (mainParts.Length == 2)
					defaultValue = mainParts[1].Trim();
				var lastSpace = propDecleration.LastIndexOf(" ");
				if (lastSpace == -1)
					throw new Exception("Illegal property decleration: '" + prop + "'");
				var type = propDecleration.Substring(0, lastSpace).Trim();
				var name = propDecleration.Substring(lastSpace).Trim();
				file.Properties.Add(name, new ViewProperty(name, type, defaultValue));
			}
		}
	}
}
