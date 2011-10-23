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

namespace Castle.MonoRail.Views.AspView.Tests.Compiler.PreCompilationSteps
{
	using AspView.Compiler.PreCompilationSteps;
	using NUnit.Framework;

	[TestFixture]
	public class ScriptToCodeTransformerTestFixture
	{
		readonly ScriptToCodeTransformer scriptTransformer = new ScriptToCodeTransformer();

		[Test]
		public void Transform_WhenThereIsOnlyMarkup_WorksWell()
		{
			var source = @"Markup1
Markup2
Markup3";
			var expected = @"Output(@""Markup1
Markup2
Markup3"");
";

			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WhenThereIsOnlyCode_WorksWell()
		{
			var source = @"<%
string[] strings = new string[] { ""Hello1"", ""Hello2"" };
foreach (string s in strings)
	Output(s);
%>";

			var expected = @"string[] strings = new string[] { ""Hello1"", ""Hello2"" };
foreach (string s in strings)
	Output(s);
";

			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WhenStartsWithMarkupAndEndsWithMarkup_Transforms()
		{
			var source = @"Markup1
<% string s1; %>
Markup2
<% string s2; %>
Markup3";
			var expected = @"Output(@""Markup1
"");
string s1;
Output(@""
Markup2
"");
string s2;
Output(@""
Markup3"");
";

			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WhenStartsWithCodeAndEndsWithMarkup_Transforms()
		{
			var source = @"<% string s1; %>
Markup1
<% string s2; %>
Markup2";
			var expected = @"string s1;
Output(@""
Markup1
"");
string s2;
Output(@""
Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WhenStartsWithMarkupAndEndsWithCode_Transforms()
		{
			var source = @"Markup1
<% string s1; %>
Markup2
<% string s2; %>";
			var expected = @"Output(@""Markup1
"");
string s1;
Output(@""
Markup2
"");
string s2;
";

			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WhenStartsWithCodeAndEndsWithCode_Transforms()
		{
			var source = @"<% string s1; %>
Markup1
<% string s2; %>";
			var expected = @"string s1;
Output(@""
Markup1
"");
string s2;
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_OutputShorthand_RendersInline()
		{
			var source = @"Markup1<%=code%>Markup2";
			var expected = @"Output(@""Markup1"");
Output(code);
Output(@""Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_OutputShorthandWhenFollowedByNewLine_KeepsFollowingNewline()
		{
			var source = @"Markup1<%=code%>
Markup2";
			var expected = @"Output(@""Markup1"");
Output(code);
Output(@""
Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_OutputShorthandWhenPreceededWithNewLine_KeepsPreceedingNewline()
		{
			var source = @"Markup1
<%=code%>Markup2";
			var expected = @"Output(@""Markup1
"");
Output(code);
Output(@""Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WithDoubleQuotesInMarkup_HandledCorrectly()
		{
			var source = @"Mark""up1
<%=code%>Markup2";
			var expected = @"Output(@""Mark""""up1
"");
Output(code);
Output(@""Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_WithDoubleQuotesInCode()
		{
			var source = @"Markup1
<%=""code""%>Markup2";
			var expected = @"Output(@""Markup1
"");
Output(""code"");
Output(@""Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}

		[Test]
		public void Transform_CodeWithSharp_TreatedAsEncodedOutput()
		{
			var source = @"Markup1
<%# var %>Markup2";
			var expected = @"Output(@""Markup1
"");
OutputEncoded(var);
Output(@""Markup2"");
";
			var transformed = scriptTransformer.Transform(source);

			Assert.AreEqual(expected, transformed);
		}
	}
}
