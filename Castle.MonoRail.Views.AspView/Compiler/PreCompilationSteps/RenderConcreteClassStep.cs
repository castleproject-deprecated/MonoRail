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
	using Internal;

	public class RenderConcreteClassStep : IPreCompilationStep
	{
		private const string assemblyNamespace = "CompiledViews";

		public void Process(SourceFile file)
		{
			var writer = new TabbedStringWriter();

			foreach (var import in file.Imports)
				writer.WriteLine("using {0};", import);

			writer.WriteLine("namespace {0}", assemblyNamespace);
			writer.WriteLine("{");
			writer.Indent();
			writer.WriteLine("public class {0} : {1}", file.ClassName, file.BaseClassName);
			writer.WriteLine("{");
			writer.Indent();
			writer.WriteLine(@"protected override string ViewName {{ get {{ return ""{0}""; }} }}",
				file.ViewName.Replace("\\", "\\\\"));

			writer.WriteLine(@"protected override string ViewDirectory {{ get {{ return ""{0}""; }} }}",
				GetDirectory(file.ViewName).Replace("\\", "\\\\"));
			writer.WriteLine();

			foreach (var name in file.Properties.Keys)
			{
				var prop = file.Properties[name];
				var defaultValueString =
					prop.DefaultValue != null ? ", " + prop.DefaultValue : string.Empty;
				writer.WriteLine(
					@"private {0} {1} {{ get {{ return ({0})GetParameter(""{1}""{2}); }} }}",
					prop.Type, prop.Name, defaultValueString);
			}
			writer.WriteLine();

			writer.WriteLine("public override void Render()");
			writer.WriteLine("{");
			writer.Indent();
			writer.WriteLineWithNoIndentation(file.RenderBody);
			writer.UnIndent();
			writer.WriteLine("}");
			writer.WriteLine();
			foreach (var handlerName in file.ViewComponentSectionHandlers.Keys)
			{
				var content = file.ViewComponentSectionHandlers[handlerName];
				writer.WriteLine("internal void {0} ()", handlerName);
				writer.WriteLine("{");
				writer.Indent();
				writer.WriteLine(content);
				writer.UnIndent();
				writer.WriteLine("}");
				writer.WriteLine();
			}

			// render each embeded script block as raw class members
			foreach (var block in file.EmbededScriptBlocks) 
			{
				writer.WriteLine(block);
			}

			writer.UnIndent();
			writer.WriteLine("}");

			writer.UnIndent();
			writer.WriteLine("}");
			file.ConcreteClass = writer.GetStringBuilder().ToString();
		}

		private static string GetDirectory(string viewName)
		{
			var lastSlash = viewName.LastIndexOf('\\');
			if (lastSlash == -1)
				return viewName;
			return viewName.Substring(0, lastSlash);
		}

	}
}
