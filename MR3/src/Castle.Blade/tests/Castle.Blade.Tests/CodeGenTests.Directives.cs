namespace Castle.Blade.Tests
{
    using System;
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void InheritsStmtWithGenerics()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits Blade.ViewPage<IEnumerable<IGrouping<int,Summary>>>
<html>
    @DoSomething(10)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
			// Console.WriteLine(typeAsString);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Blade . ViewPage < IEnumerable < IGrouping < int , Summary > > > { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
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
			var normalizedCode = Normalize(typeAsString);
			// DebugWrite(normalizedCode);

			Assert.AreEqual(
@"namespace Blade { public class Generated_Type : My . BaseClass { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""\r\n<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
		}

        [Test]
        public void InheritsStmt2()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits My.BaseClass
<html>
    @DoSomething(10)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : My . BaseClass { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting: Type name or namespace as literal")]
        public void InheritsStmt_Fail()
        {
            ParseAndGenString(
@"
@inherits
<html>
    @DoSomething(10)
</html>"
);
        }

        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting: Type name or namespace as literal")]
        public void InheritsStmt_Fail2()
        {
            ParseAndGenString(
@"
@inherits ;
<html>
    @DoSomething(10)
</html>"
);
        }

        [Test]
        public void ImportNamespace1()
        {
            var typeAsString = ParseAndGenString(
@"
@using My.Namespace;
<html>
    @DoSomething(10)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { using My . Namespace ; public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""\r\n<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void ImportNamespace2()
        {
            var typeAsString = ParseAndGenString(
@"
@using My.Namespace
<html>
    @DoSomething(10)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { using My . Namespace ; public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void ImportNamespace3()
        {
            var typeAsString = ParseAndGenString(
@"
@using Namespace
<html>
    @DoSomething(10)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { using Namespace ; public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<html>\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        // [Test, ExpectedException(typeof(Exception))]
        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting: Type name or namespace as literal")]
        public void ImportNamespace_Invalid()
        {
            ParseAndGenString(
@"
@using ;
<html>
    @DoSomething(10)
</html>"
);
        }

        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting: Type name or namespace as literal")]
        public void ImportNamespace_Invalid2()
        {
            var typeAsString = ParseAndGenString(
@"
@using
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
