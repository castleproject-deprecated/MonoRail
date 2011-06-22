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
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void MarkupWithSelfTerminatingElement_NoTransition()
        {
            var content =
@"@{ 
	<link src=""this is a source"" rel=""something"" />
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        [ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "Expecting closing tag </link>. If you intentionally didn't close it, put the")]
        public void MarkupWithoutClosingTag()
        {
            var content =
@"@{ 
	<link src=""this is a source"" rel=""something""> aa </a>
}";
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
</html>
";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
