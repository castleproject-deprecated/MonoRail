namespace Castle.Blade.Tests
{
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void EmptyContentIsAccepted()
        {
            var list = Parser.parse_string("");
            Assert.IsTrue( list.Count() == 0 );
        }

        [Test]
        public void OnlyStaticContent()
        {
            string content = "<html> something </html>";
            var list = Parser.parse_string(content);
            Assert.IsTrue(list.Count() == 1);
            Assert.AreEqual(content, ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
        }

//        [Test]
//        public void StaticWithSingleTransition()
//        {
//            string content = "<html> @something </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//            
//            Assert.AreEqual("something", list.ElementAt(1).AsInvocation().Item1);
//            Assert.IsTrue(list.ElementAt(1).AsInvocation().Item2.IsNone);
//
//            Assert.AreEqual(" </html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void StaticWithSingleTransitionInParenthesis()
//        {
//            string content = "<html> @(something) </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//            Assert.AreEqual("something", ((AST.ASTNode.Code)list.ElementAt(1)).Item);
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void DotAfterExpIsNotPartOfExp()
//        {
//            string content = "<html> @something. </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//            Assert.AreEqual("something", list.ElementAt(1).AsInvocation().Item1);
//            Assert.AreEqual(". </html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void Comments()
//        {
//            string content = "<html> @* Server side comments here *@ </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//            Assert.IsTrue(list.ElementAt(1).IsComment);
//            Assert.AreEqual(" </html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void DelegateInvocation1()
//        {
//            string content = "<html> @b() </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item1);
//            Assert.IsTrue(list.ElementAt(1).AsInvocation().Item2.IsParam);
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void DelegateInvocation2()
//        {
//            string content = "<html> @b(1) </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item1);
//            Assert.IsTrue(list.ElementAt(1).AsInvocation().Item2.IsParam);
//            Assert.AreEqual("1", list.ElementAt(1).AsInvocation().Item2.AsParam().Item.Head.AsCode().Item);
//            
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void DelegateInvocation3()
//        {
//            string content = "<html> @b(\"Bold this\") </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item1);
//            Assert.IsTrue(list.ElementAt(1).AsInvocation().Item2.IsParam);
//            Assert.AreEqual("\"Bold this\"", list.ElementAt(1).AsInvocation().Item2.AsParam().Item.Head.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void MemberInvocation1()
//        {
//            string content = "<html> @a.b </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            Assert.AreEqual("a", list.ElementAt(1).AsInvocation().Item1);
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item2.AsMember().Item);
//
//            Assert.AreEqual(" </html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void MemberInvocation2()
//        {
//            string content = "<html> @a.b.c </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            Assert.AreEqual("a", list.ElementAt(1).AsInvocation().Item1);
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item2.AsInvocation().Item1);
//            Assert.AreEqual("c", list.ElementAt(1).AsInvocation().Item2.AsInvocation().Item2.AsMember().Item);
//
//            Assert.AreEqual(" </html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void MethodInvocation1()
//        {
//            string content = "<html> @a.b.c() </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            Assert.AreEqual("a", list.ElementAt(1).AsInvocation().Item1);
//            Assert.AreEqual("b", list.ElementAt(1).AsInvocation().Item2.AsInvocation().Item1);
//            Assert.AreEqual("c", list.ElementAt(1).AsInvocation().Item2.AsInvocation().Item2.AsInvocation().Item1);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void ForeachBlock1()
//        {
//            string content = "<html> @foreach(var xx in Items) \r\n { \r\n  } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsKeywordConditionalBlock();
//            Assert.AreEqual("foreach", keyBlock.Item1);
//            Assert.AreEqual("var xx in Items", keyBlock.Item2.AsCode().Item);
//            Assert.AreEqual("", keyBlock.Item3.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void ForBlock1()
//        {
//            string content = "<html> @for(var i=0; i<100; i++) { \r\n  } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsKeywordConditionalBlock();
//            Assert.AreEqual("for", keyBlock.Item1);
//            Assert.AreEqual("var i=0; i<100; i++", keyBlock.Item2.AsCode().Item);
//            Assert.AreEqual("", keyBlock.Item3.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void IfElseBlock1()
//        {
//            string content = "<html> @if(x == 10) \r\n { code } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsIfElseBlock();
//            Assert.AreEqual("x == 10", keyBlock.Item1.AsCode().Item);
//            Assert.AreEqual("code ", keyBlock.Item2.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void IfElseBlock2()
//        {
//            string content = "<html> @if(x == 10) { code } else { code2 } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsIfElseBlock();
//            Assert.AreEqual("x == 10", keyBlock.Item1.AsCode().Item);
//            Assert.AreEqual("code ", keyBlock.Item2.AsCode().Item);
//            Assert.AreEqual("code2 ", keyBlock.Item3.Value.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void IfElseBlock3()
//        {
//            string content = "<html> @if(x == 10) \r\n { code } else if (x == 2) { code2 } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsIfElseBlock();
//            Assert.AreEqual("x == 10", keyBlock.Item1.AsCode().Item);
//            Assert.AreEqual("code ", keyBlock.Item2.AsCode().Item);
//            Assert.AreEqual("x == 2", keyBlock.Item3.Value.AsIfElseBlock().Item1.AsCode().Item);
//            Assert.AreEqual("code2 ", keyBlock.Item3.Value.AsIfElseBlock().Item2.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void IfElseBlock4()
//        {
//            string content = "<html> @if(x == 10) \r\n { code } else if (x == 2) { code2 } \r\nelse\r\n{ code 3} \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsIfElseBlock();
//            Assert.AreEqual("x == 10", keyBlock.Item1.AsCode().Item);
//            Assert.AreEqual("code ", keyBlock.Item2.AsCode().Item);
//            
//            var ifelseBlock = keyBlock.Item3.Value.AsIfElseBlock();
//            Assert.AreEqual("x == 2", ifelseBlock.Item1.AsCode().Item);
//            Assert.AreEqual("code2 ", ifelseBlock.Item2.AsCode().Item);
//            
//            var elseBlock = ifelseBlock.Item3.Value.AsCode();
//            Assert.AreEqual("code 3", elseBlock.Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }
//
//        [Test]
//        public void LockBlock1()
//        {
//            string content = "<html> @lock(x) \r\n { code } \r\n </html>";
//            var list = Parser.parse_string(content);
//            Assert.IsTrue(list.Count() == 3);
//            Assert.AreEqual("<html> ", ((AST.ASTNode.Markup)list.ElementAt(0)).Item);
//
//            var keyBlock = list.ElementAt(1).AsKeywordConditionalBlock();
//            Assert.AreEqual("lock", keyBlock.Item1);
//            Assert.AreEqual("x", keyBlock.Item2.AsCode().Item);
//            Assert.AreEqual("code ", keyBlock.Item3.AsCode().Item);
//
//            Assert.AreEqual("</html>", ((AST.ASTNode.Markup)list.ElementAt(2)).Item);
//        }

    }

    internal static class Exts
    {
        internal static AST.ASTNode.Invocation AsInvocation(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsInvocation, "Element isn't an Invocation instance");
            return (AST.ASTNode.Invocation)el;
        }

        internal static AST.ASTNode.Param AsParam(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsParam, "Element isn't a Param instance");
            return (AST.ASTNode.Param)el;
        }

        internal static AST.ASTNode.Code AsCode(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsCode, "Element isn't a Code instance");
            return (AST.ASTNode.Code)el;
        }

        

        internal static AST.ASTNode.CodeBlock AsCodeBlock(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsCodeBlock, "Element isn't a CodeBlock instance");
            return (AST.ASTNode.CodeBlock)el;
        }

        internal static AST.ASTNode.KeywordConditionalBlock AsKeywordConditionalBlock(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsKeywordConditionalBlock, "Element isn't a KeywordConditionalBlock instance");
            return (AST.ASTNode.KeywordConditionalBlock)el;
        }

        internal static AST.ASTNode.IfElseBlock AsIfElseBlock(this AST.ASTNode el)
        {
            Assert.IsNotNull(el);
            Assert.IsTrue(el.IsIfElseBlock, "Element isn't an IfElseBlock instance");
            return (AST.ASTNode.IfElseBlock)el;
        }
    }
}
