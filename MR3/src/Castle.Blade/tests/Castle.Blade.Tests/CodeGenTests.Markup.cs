namespace Castle.Blade.Tests
{
    using System;
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void MarkupWithSelfTerminatingElement()
        {
            var content =
@"@{ 
	<link src=""@x"" />
}";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""<link src=\"""" ) ; 
    Write ( x ) ; 
    WriteLiteral ( ""\"" />"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void MarkupWithSelfTerminatingElement_NoTransition()
        {
            var content =
@"@{ 
	<link src=""this is a source"" rel=""something"" />
}";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""<link src=\""this is a source\"" rel=\""something\"" />"" ) ; } } } ", normalizedCode);
        }

        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting closing tag </link>. If you intentionally didn't close it, put the")]
        public void MarkupWithoutClosingTag()
        {
            var content =
@"@{ 
	<link src=""this is a source"" rel=""something""> aa </a>
}";
            ParseAndGenString(content);
        }

        [Test]
        public void ContentWithAtAt()
        {
            var content = @" @@ </html>";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( "" @ </html>"" ) ; } } } ", normalizedCode);
        }
    }
}
