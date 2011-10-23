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


namespace Castle.MonoRail.Views.AspView.Configuration
{
	using System;
	using Compiler;

	public class AspViewEngineOptions
	{
		readonly AspViewCompilerOptions compilerOptions;

		public AspViewEngineOptions() : this(new AspViewCompilerOptions())
		{
		}

		public AspViewEngineOptions(AspViewCompilerOptions compilerOptions)
		{
			this.compilerOptions = compilerOptions;
			ViewProperties = ViewPropertiesInclusionOptions.RequestParams;
		}

		/// <summary>
		/// The compiler's options object
		/// </summary>
		public AspViewCompilerOptions CompilerOptions
		{
			get { return compilerOptions; }
		}

		/// <summary>
		/// Tells the view engine, which parts of the RequestParams to include in each view's properties collection
		/// </summary>
		public ViewPropertiesInclusionOptions ViewProperties { get; set; }

		public bool Include(ViewPropertiesInclusionOptions option)
		{
			return (ViewProperties & option) == option;
		}
	}

	[Flags]
	public enum ViewPropertiesInclusionOptions
	{
		None = 0,

        QueryString = 1,
		
		Form = 2,
				
		/// <summary>
		/// The HttpRequest.Params collection.
		/// this will include QueryString, Form, Cookies and ServerVars
		/// </summary>
		RequestParams = 15
	}
}