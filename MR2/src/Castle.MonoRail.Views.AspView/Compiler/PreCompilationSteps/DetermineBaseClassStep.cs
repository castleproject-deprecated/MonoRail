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

namespace Castle.MonoRail.Views.AspView.Compiler.PreCompilationSteps
{
	using System.Text.RegularExpressions;

	public class DetermineBaseClassStep : IPreCompilationStep
	{
		public static readonly string DefaultDesignTimeClassName = "Castle.MonoRail.Views.AspView.ViewAtDesignTime";
		public static readonly string DefaultBaseClassName = "AspViewBase";
		public static readonly string DesignTimeClassPrefix = "AtDesignTime";

		public void Process(SourceFile file)
		{
			file.RenderBody = Internal.RegularExpressions.PageDirective.Replace(file.RenderBody, delegate(Match match)
			{
				file.BaseClassName = GetBaseClass(match.Groups["base"]);
				file.TypedViewName = GetTypedViewName(match.Groups["view"]);
				if (file.TypedViewName != null)
					file.BaseClassName += "<" + file.TypedViewName + ">";
				return string.Empty;
			}, 1);

			file.RenderBody = Internal.RegularExpressions.MasterPageDirective.Replace(file.RenderBody, delegate(Match match)
			{
				file.BaseClassName = GetBaseClass(match.Groups["base"]);
				file.TypedViewName = GetTypedViewName(match.Groups["view"]);
				if (file.TypedViewName != null)
					file.BaseClassName += "<" + file.TypedViewName + ">";
				return string.Empty;
			}, 1);
		}

		private static string GetTypedViewName(Capture view)
		{
			if (view == null || string.IsNullOrEmpty(view.Value))
				return null;

			// strip .NET standard generic notation

			if (view.Value.IndexOf('`')<0)
			{
				if (view.Value.IndexOf(',') > -1)
					throw new AspViewException("Basetype for views cannot be of a generic type with more than a single type parameter. found \"{0}\"", view.Value);
				
				return view.Value;
			}

			for (var i=2; i <=10; ++i)
				if (view.Value.Contains("`"+i))
					throw new AspViewException("Basetype for views cannot be of a generic type with more than a single type parameter. found \"{0}\"", view.Value);

			var viewTypeName = view.Value
				.Replace("`1[[", "<")
				.Replace("`1[", "<")
				.Replace("]]", ">")
				.Replace("]", ">");
			
			return viewTypeName.Split(new[]{','})[0].Trim();
		}

		private static string GetBaseClass(Capture baseClass)
		{
			if (baseClass == null)
				return DefaultBaseClassName;

			if (string.IsNullOrEmpty(baseClass.Value))
				return DefaultBaseClassName;

			if (baseClass.Value == DefaultDesignTimeClassName)
				return DefaultBaseClassName;

			var extractedName = baseClass.Value;

			if (extractedName.EndsWith(DesignTimeClassPrefix))
				extractedName = extractedName.Substring(0, extractedName.Length - DesignTimeClassPrefix.Length);

			return extractedName;
		}
	}
}
