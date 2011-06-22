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
