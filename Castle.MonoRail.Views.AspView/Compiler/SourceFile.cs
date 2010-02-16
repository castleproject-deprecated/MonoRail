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

	/// <summary>
	/// Represents a view (template) file
	/// </summary>
	public class SourceFile
	{
		///<summary>
		/// A new template file
		///</summary>
		public SourceFile()
		{
			Imports = new HashSet<string>();
			EmbededScriptBlocks = new List<string>();
			Properties = new Dictionary<string, ViewProperty>();
			ViewComponentSectionHandlers = new Dictionary<string, string>();
		}

		///<summary>
		/// The view's name
		///</summary>
		public string ViewName { get; set; }

		/// <summary>
		/// The template source of the view 
		/// </summary>
		public string ViewSource { get; set; }

		/// <summary>
		/// The body of the Render() method
		/// </summary>
		public string RenderBody { get; set; }

		/// <summary>
		/// The view's class name
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// The base class for the view
		/// </summary>
		public string BaseClassName { get; set; }

		/// <summary>
		/// The view, with a typed access to the propertybag
		/// </summary>
		public string TypedViewName { get; set; }

		/// <summary>
		/// The transformed, c# code of the class
		/// </summary>
		public string ConcreteClass { get; set; }

		/// <summary>
		/// The filename for the output
		/// </summary>
		public string FileName
		{
			get { return ClassName + AbstractCompiler.ViewSourceFileExtension; }
		}

		/// <summary>
		/// Import declerations needed
		/// </summary>
		public HashSet<string> Imports { get; private set; }

		/// <summary>
		/// The properties for the view
		/// </summary>
		public IDictionary<string, ViewProperty> Properties { get; private set; }

		/// <summary>
		/// A bunch of handlers to deal with ViewComponents sections
		/// </summary>
		public IDictionary<string, string> ViewComponentSectionHandlers { get; private set; }

		/// <summary>
		/// contains the list of embeded scripts blocks (&lt;script runat="server"/&gt; elements contents) 
		/// that will be rendered as raw class content
		/// </summary>
		public IList<string> EmbededScriptBlocks { get; private set; }
	}
}
