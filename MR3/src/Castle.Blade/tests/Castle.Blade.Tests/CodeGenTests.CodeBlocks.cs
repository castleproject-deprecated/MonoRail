namespace Castle.Blade.Tests
{
    using System;
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void BracketAfterId1()
        {
            var content =
@"
                    @{
                        var id = (int) @Bag[""id""];
                    }
                    
                    <div class=""asset"" data-symbol=""@Model"">
";

            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n                    "" ) ; var id = ( int ) @Bag [ ""id"" ] ; 
    WriteLiteral ( ""\r\n                    \r\n                    <div class=\""asset\"" data-symbol=\"""" ) ; 
    Write ( Model ) ; 
    WriteLiteral ( ""\"">\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void InlineContentWithinCall2()
        {
            var content =
@"
                    @{
                        Form.Helper( @<b></b> );
                    }
                    
                    <div class=""asset"" data-symbol=""@Model"">
";
            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n                    "" ) ; Form . Helper ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""</b>"" ) ; } ) ) ; 
    WriteLiteral ( ""\r\n                    \r\n                    <div class=\""asset\"" data-symbol=\"""" ) ; 
    Write ( Model ) ; 
    WriteLiteral ( ""\"">\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test, ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "creates a lambda, so it needs to be used in the context of assignment or")]
        public void CannotUseAtElementAsLineStarterInCodeBlock()
        {
            var content =
@"
                    @{
                        @<b>this will fail</b>
                    }
";
            ParseAndGenString(content);
        }

        [Test, ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "creates a lambda, so it needs to be used in the context of assignment or")]
        public void CannotUseAtElementAsLineStarterAfterSemiColonInCodeBlock()
        {
            var content =
@"
                    @{
                        for(something) { }; @<b>this will fail</b>
                    }
";
            ParseAndGenString(content);
        }

        [Test]
        public void CodeBlockAndContent1()
        {
            var content = 
@"<html> @{ LayoutName = ""test""; }";
            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""<html> "" ) ; LayoutName = ""test"" ; } } } ", normalizedCode);
        }

        [Test]
        public void CodeBlockAndInlineText()
        {
            var typeAsString = ParseAndGenString(
@"
<div @{ if (index > 5) { @:src=""display: none"" 
 } }>
");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n<div "" ) ; if ( index > 5 ) { 
    WriteLiteral ( ""src=\""display: none\"" "" ) ; } 
    WriteLiteral ( "">\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void CodeBlockWithContent1()
        {
            var typeAsString = ParseAndGenString(
@"<b>@{ 
    var x = Helper.Form( x, 10, ""test"", '1' );
}</b>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""<b>"" ) ; var x = Helper . Form ( x , 10 , ""test"" , '1' ) ; 
    WriteLiteral ( ""</b>"" ) ; } } } ", normalizedCode);
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
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""something"" ) ; 
    Write ( x ) ; 
    WriteLiteral ( "" \r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void ContentInsideCodeBlockWithTransition1()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
    <script src=""@(x)"">something</script> 
} 
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<script src=\"""" ) ; 
    Write ( x ) ; 
    WriteLiteral ( ""\"">"" ) ; 
    WriteLiteral ( ""something</script>"" ) ; 
    WriteLiteral ( "" \r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void ContentInsideCodeBlockWithTransition2()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
  <b>something@(x)</b> 
} 
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""<b>"" ) ; 
    WriteLiteral ( ""something"" ) ; 
    Write ( x ) ; 
    WriteLiteral ( ""</b>"" ) ; 
    WriteLiteral ( "" \r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void CodeBlockWithTextMarkup1()
        {
            var typeAsString = ParseAndGenString(
@"@for(var index = 0; i < 100; i++)
{
	if (index == 0 || index % 5 == 0)
	{
		<text><div @{ if (index > 5) { 
						  <text>src=""display: none"" </text>
					  }
				   } ></text>
	}
	index++;

	<div>
		<div>@watchItem</div>
	</div>

	if (index == 0 || index % 5 == 0)
	{
		<text></div></text>
	}
}
");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);
            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { for ( var index = 0 ; i < 100 ; i ++ ) { if ( index == 0 || index % 5 == 0 ) { 
    WriteLiteral ( ""<div "" ) ; if ( index > 5 ) { 
    WriteLiteral ( ""src=\""display: none\"" "" ) ; } 
    WriteLiteral ( "" >"" ) ; } index ++ ; 
    WriteLiteral ( ""<div>"" ) ; 
    WriteLiteral ( ""\r\n\t\t<div>"" ) ; 
    Write ( watchItem ) ; 
    WriteLiteral ( ""</div>\r\n\t</div>"" ) ; if ( index == 0 || index % 5 == 0 ) { 
    WriteLiteral ( ""</div>"" ) ; } } 
    WriteLiteral ( ""\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void CodeBlockWithTextMarkup2()
        {
            var typeAsString = ParseAndGenString(
@"@{
	<div>
		<div>@watchItem</div>
	</div>
}
");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    WriteLiteral ( ""<div>"" ) ; 
    WriteLiteral ( ""\r\n\t\t<div>"" ) ; 
    Write ( watchItem ) ; 
    WriteLiteral ( ""</div>\r\n\t</div>"" ) ; 
    WriteLiteral ( ""\r\n"" ) ; } } } ", normalizedCode);
        }
    }
}
