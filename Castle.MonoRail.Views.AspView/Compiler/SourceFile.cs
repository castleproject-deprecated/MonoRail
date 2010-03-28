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


namespace Castle.MonoRail.Views.AspView.Compiler
{
	using System.Collections.Generic;
    using Internal;

	public class SourceFile
	{
		private readonly StringSet imports = new StringSet();
		private readonly IList<string> embededScriptBlocks = new List<string>();
		private readonly IDictionary<string, ViewProperty> properties = new Dictionary<string, ViewProperty>();
		private readonly IDictionary<string, string> viewComponentSectionHandlers = new Dictionary<string, string>();

		public string ViewName { get; set; }

		public string ViewSource { get; set; }

		public string RenderBody { get; set; }

		public string ClassName { get; set; }

		public string BaseClassName { get; set; }

		public string TypedViewName { get; set; }

		public string ConcreteClass { get; set; }

		public string FileName
		{
			get { return ClassName + AbstractCompiler.ViewSourceFileExtension; }
		}

		public StringSet Imports
		{
			get { return imports; }
		}

		public IDictionary<string, ViewProperty> Properties
		{
			get { return properties; }
		}

		public IDictionary<string, string> ViewComponentSectionHandlers
		{
			get { return viewComponentSectionHandlers; }
		}

		/// <summary>
		/// contains the list of embeded scripts blocks (&lt;script runat="server"/&gt; elements contents) 
		/// that will be rendered as raw class content
		/// </summary>
		public IList<string> EmbededScriptBlocks 
		{
			get { return embededScriptBlocks; }
		}
	}
}
