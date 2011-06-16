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
        public void CodeGenForContentOnly()
        {
            var decl = CodeGen.generate_code(new[] { AST.ASTNode.NewMarkup("hello world") });
            System.Diagnostics.Debug.WriteLine(DeclToString(decl));
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
            var typeAsString = ParseAndGenString("@if(x == 10) { <text>something</text>  }  </html>");
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
        public void CodeBlockAndContent4()
        {
            var typeAsString = ParseAndGenString(
@"@{ 
    <text>something@(x)</text> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent5()
        {
            var typeAsString = ParseAndGenString("@{ <b>something@(x)</b> \r\n  } \r\n</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition1()
        {
            var typeAsString = ParseAndGenString("<b>@x</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);

            // Assert.AreEqual("", DeclToString(decl));
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


        private static string ParseAndGenString(string input)
        {
            var nodes = Parser.parse(input);
            var decl = CodeGen.generate_code(nodes);
            return DeclToString(decl);
        }

        private static string DeclToString(CodeTypeDeclaration decl)
        {
            var provider = new CSharpCodeProvider();
            var opts = new CodeGeneratorOptions {IndentString = " ", BracingStyle = "C"};

            var writer = new StringWriter();
            provider.GenerateCodeFromType(decl, writer, opts);
            return writer.GetStringBuilder().ToString();
        }
    }
}
