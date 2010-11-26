#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.MonoRail.WebForms
{
	using System.CodeDom;
	using System.Web.UI;

	internal sealed class ViewPageControlBuilder : FileLevelPageControlBuilder
	{
		public string PageBaseType { get; set; }

		public override void ProcessGeneratedCode(
			CodeCompileUnit codeCompileUnit,
			CodeTypeDeclaration baseType,
			CodeTypeDeclaration derivedType,
			CodeMemberMethod buildMethod,
			CodeMemberMethod dataBindingMethod)
		{
			// If we find got a base class string, use it
			if (PageBaseType != null)
			{
				derivedType.BaseTypes[0] = new CodeTypeReference(PageBaseType);
			}
		}
	}
}