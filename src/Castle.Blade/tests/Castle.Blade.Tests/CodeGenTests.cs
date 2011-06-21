using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Castle.Blade.Tests
{
    [TestFixture]
    public class CodeGenTests
    {
        [Test]
        public void ContentOnlySection()
        {
            var content =
@"

@section AdditionalStyles
{
    .ui-corner-all { -moz-border-radius: 4px; -webkit-border-radius: 4px; border-radius: 4px; }
}

";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup1()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string, object>();
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup2()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string>();
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup3()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string, object>(); <p>sss</p>
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup4()
        {
            var content =
@"@{ 
	<p>sss</p> var parameters = new Dictionary<string, object>();
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IdentifiersAreKeptIsolated()
        {
            var content =
@"
@pageSize
@pageSize<
@pageSize!
@pageSize-
@pageSize}
@pageSize)
@pageSize.
@pageSize.<a>test<a>
@pageSize{
@pageSize[
@pageSize>
@pageSize,
@pageSize;
@pageSize:
";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void TransitionsWithinQuotes()
        {
            var content =
                @"
@section AdditionalHeader
{
    <!--[if lte IE 8]><script language=""javascript"" type=""text/javascript"" src=""@Url.Content(""/Content/Scripts/excanvas/excanvas.min.js"")""></script><![endif]-->
    <script src=""@Url.Content(""/Content/Scripts/json2.js"")""></script>  
}

<html />
";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }


        [Test]
        public void ContentWithAtAt()
        {
            var content =
@"@@modeltype
@{
    Layout = ""~/Views/Shared/default.cshtml"";
}

<div class=""container"">
    <div class=""header"">
        <h1>Default Home Page - </h1>
        <h3>Well @name, there's nothing on earth like a genuine, bona fide, Electrified, Six-car Monorail!</h3>
    </div>

    @using (Form.BeginForm(WebApplication1.Controllers.TodoController.Urls.Create()))
    {
        <fieldset id=""contactForm"">
            <legend>Message</legend>
            <p>
                @Html.Label(""Email"", ""Email""): 
                @Html.TextInput(""Email"")
            </p>
            <p>
                <input type=""submit"" value=""Send"" />
            </p>
        </fieldset>
    } 
</div>";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent1()
        {
            var typeAsString = ParseAndGenString("<html> @{ LayoutName = \"test\"; }");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) 
{ 
    @:text 
}  
</html> 
"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent2()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something</text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent21()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text></text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent22()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something<b>with</b></text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent23()
        {
            var typeAsString = ParseAndGenString(
@"
@if(x == 10) 
{ 
    <text>something<b>@x</b></text>  
}  
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent3()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { 
    if (x++ == 11) 
    { 
        call();
    } 
    <text>something</text> 
    Debug.WriteLine(""some""); 
} 
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent4()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { 
    <text>something</text> 
} else { 
    <text>else</text> 
}
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent5()
        {
            var typeAsString = ParseAndGenString(
@"
@if(x == 10) { 
    <text>something</text> 
} else if (x == 20) { 
    <text>else</text> 
}
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent4()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
    <text>something@(x)</text> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent5()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
  <b>something@(x)</b> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition1()
        {
            var typeAsString = ParseAndGenString(
@"<b>@x</b>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition2()
        {
            var typeAsString = ParseAndGenString("<b>@(x)</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition3()
        {
            var typeAsString = ParseAndGenString("<b>@Helper.Form()</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);

            // Assert.AreEqual("", DeclToString(decl));
        }

        [Test]
        public void ContentWithTransition4()
        {
            var typeAsString = ParseAndGenString("<b>@Helper.Form( x, 10, \"test\", '1' )</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);

            // Assert.AreEqual("", DeclToString(decl));
        }

        [Test]
        public void ContentWithTransitionCodeBlock1()
        {
            var typeAsString = ParseAndGenString("<b>@{ Helper.Form( x, 10, \"test\", '1' ) }</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam1()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form(  @=> p <b>something @p</b> ); } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam2()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form(  @=> p { <b>something @p</b> } ); } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam3()
        {
            var typeAsString = ParseAndGenString("@Helper.Form(WebApplication1.Controllers.TodoController.Urls.Create(),  @=> p <fieldset id='tt'>something @p</fieldset> )  ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam4()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.FormFor(WebApplication1.Controllers.TodoController.Urls.Create(),  @=> p 
{
    <fieldset id='tt'>something @p

    @p.FieldsFor( @=> p2 {
        <fieldset>
        @p2.FieldFor(""some"")
        </fieldset>
    })

    </fieldset> 

})"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam5()
        {
            var typeAsString = ParseAndGenString(
@"
@Helper.FormFor( @=> p {
    <fieldset id='tt'>something</fieldset> 
})"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }


        [Test]
        public void BlockWithLambda1()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form( @<b>something</b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda2()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form( @<b><a>something</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda3()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form( @<b><a>something@x</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda4()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form( @<b><a>something@(x)</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CallWithLambda1()
        {
            var typeAsString = ParseAndGenString("@Helper.Form( @<b><a>something@(x)</a></b> )  ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambdaAssignment()
        {
            var typeAsString = ParseAndGenString("@{\r\n var x = @<b><a>something</a></b>; \r\n } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockWithContent1()
        {
            var typeAsString = ParseAndGenString(
@"<b>@{ 
    var x = Helper.Form( x, 10, ""test"", '1' )
}</b>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void Section_Empty()
        {
            var typeAsString = ParseAndGenString(
@"<html>
@section Name
{
    <somecontent> here </somecontent>
}
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void FunctionsBlock()
        {
            var typeAsString = ParseAndGenString(
@"<html>
@functions {
        string DoSomething(int x) { 
            return ""aa"";
        }
}
    @DoSomething(10)
    @DoSomething(20)

</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void InheritsStmt1()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits My.BaseClass;
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void HelperDecl1()
        {
            var typeAsString = ParseAndGenString(
@"
@helper Name () { 
    <text>hello</text>
}
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void HelperDecl2()
        {
            var typeAsString = ParseAndGenString(
@"
@helper ExpXml (string xml, int depth) { 
    <text>hello</text> @xml
}
<html>
    @ExpXml(""<testing>"", 1)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }


        private static string ParseAndGenString(string input)
        {
            var node = Parser.parse_string(input);
            var decl = CodeGen.GenerateCodeFromAST("_Generated_Type", node, new CodeGenOptions());
            return DeclToString(decl);
        }

        private static string DeclToString(CodeCompileUnit decl)
        {
            var provider = new CSharpCodeProvider();
            var opts = new CodeGeneratorOptions { IndentString = "    ", BracingStyle = "C" };

            var writer = new StringWriter();
            provider.GenerateCodeFromCompileUnit(decl, writer, opts);
            return writer.GetStringBuilder().ToString();
        }
    }
}
