using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Castle.Blade.Tests
{
    using System;
    using System.Text;
    using Antlr.Runtime.Tree;
    using Castle.Blade.Tests.TestFx;

    [TestFixture]
    public partial class CodeGenTests
    {

        private static string ParseAndGenString(string input)
        {
            var node = Parser.parse_string(input);
            var decl = CodeGen.GenerateCodeFromAST("Generated_Type", node, new CodeGenOptions());
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

        private string Normalize(string typeAsString)
        {
            var cunit = new Parse().ParseContent(typeAsString);
            var buf = new StringBuilder();
            var tree = cunit.Tree as ITree;

            RecursivePrint(tree, buf);

            return buf.ToString();
        }

        private static void RecursivePrint(ITree tree, StringBuilder buf)
        {
            if (!string.IsNullOrEmpty(tree.Text))
                buf.Append(tree.Text).Append(' ');

            for (var i = 0; i < tree.ChildCount; i++)
            {
                var child = tree.GetChild(i);
                RecursivePrint(child, buf);
            }
        }

        private static void DebugWrite(string content)
        {
            System.Diagnostics.Debug.WriteLine(content);
        }
    }
}
