using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Castle.Blade.Tests
{
    [TestFixture]
    public partial class CodeGenTests
    {
        [Test]
        public void ContentWithTransition1()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@x</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition2()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@(x)</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition3()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@Helper.Form()</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition4()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@Helper.Form( x, 10, ""test"", '1' )</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransitionCodeBlock1()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@{ Helper.Form( x, 10, ""test"", '1' ) }</b>");
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
