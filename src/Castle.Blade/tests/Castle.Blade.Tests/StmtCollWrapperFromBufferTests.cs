namespace Castle.Blade.Tests
{
    using System.CodeDom;
    using System.Text;
    using FParsec;
    using NUnit.Framework;

    [TestFixture]
    public class StmtCollWrapperFromBufferTests
    {
        [Test]
        public void StmtCollWrapper_FromBuffer_()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine("var x = ");
            stmts.AddLine("100");
            stmts.Flush();

            Assert.AreEqual("var x = 100\r\n", buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_FromBuffer_2()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine("var x = ");
            stmts.AddLine("100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.Flush();

            Assert.AreEqual(
@"var x = 100;
if (x == 10)
{
Debug.Print(x);
}
", buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_FromBuffer_3()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine("var x = ");
            stmts.AddLine("100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.AddLine("var x = ");
            stmts.AddLine("100;");
            stmts.Flush();

            Assert.AreEqual(
@"var x = 100;
if (x == 10)
{
Debug.Print(x);
}
var x = 100;
",
                buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_FromBuffer_4()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine("var x = ");
            stmts.Flush();
            stmts.AddLine("100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.AddLine("var x = ");
            stmts.Flush();
            stmts.AddLine("100;");
            stmts.Flush();

            Assert.AreEqual(
@"var x = 
100;
if (x == 10)
{
Debug.Print(x);
}
var x = 
100;
",
                buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_WithPosition_1()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.AddLine("100");
            stmts.Flush();

            Assert.AreEqual(
@"#line 10 ""name of file""
var x = 100
", buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_WithPosition_2()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.AddLine(new Position("name of file", 1, 10, 20), "100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.Flush();

            Assert.AreEqual(
@"#line 10 ""name of file""
var x = 
#line 10 ""name of file""
100;
if (x == 10)
{
Debug.Print(x);
}
", buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_WithPosition_3()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.AddLine("100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.AddLine("100;");
            stmts.Flush();

            Assert.AreEqual(
@"#line 10 ""name of file""
var x = 100;
if (x == 10)
{
Debug.Print(x);
}
#line 10 ""name of file""
var x = 100;
", buf.ToString());
        }

        [Test]
        public void StmtCollWrapper_WithPosition_4()
        {
            var buf = new StringBuilder();
            var stmts = CodeGen.StmtCollWrapper.FromBuffer(buf);
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.Flush();
            stmts.AddLine("100;");
            stmts.Add(new CodeConditionStatement(new CodeSnippetExpression("x == 10"), new CodeSnippetStatement("Debug.Print(x);")));
            stmts.AddLine(new Position("name of file", 1, 10, 20), "var x = ");
            stmts.Flush();
            stmts.AddLine("100;");
            stmts.Flush();

            Assert.AreEqual(
@"#line 10 ""name of file""
var x = 
100;
if (x == 10)
{
Debug.Print(x);
}
#line 10 ""name of file""
var x = 
100;
", buf.ToString());
        }
    }
}
