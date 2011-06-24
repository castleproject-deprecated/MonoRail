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

            System.Diagnostics.Debug.WriteLine(ParseAndGenString(content));
        }

        [Test]
        public void InlineContentWithinCall2()
        {
            var content =
@"
                    @{
                        Form.Helper( @<b></b> )
                    }
                    
                    <div class=""asset"" data-symbol=""@Model"">
";

            System.Diagnostics.Debug.WriteLine(ParseAndGenString(content));
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

            System.Diagnostics.Debug.WriteLine(ParseAndGenString(content));
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

            System.Diagnostics.Debug.WriteLine(ParseAndGenString(content));
        }

        [Test, ExpectedException(typeof(Exception), MatchType = MessageMatch.Contains, ExpectedMessage = "inline markup is not expected here. Maybe you should use @<element> instead?")]
        public void CannotUseAtElementAsLineStarterInCall()
        {
            var content =
@"
                    @{
                        Form.Helper( <b></b> )
                    }
";

            System.Diagnostics.Debug.WriteLine(ParseAndGenString(content));
        }


        [Test]
        public void CodeBlockAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"<html> @{ LayoutName = ""test""; }");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndInlineText()
        {
            var typeAsString = ParseAndGenString(
@"
<div @{ if (index > 5) { @:src=""display: none"" 
 } }>
");
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
        public void CodeBlockAndContent4()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
    <text>something@(x)</text> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent41()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
    <script src=""@(x)"">something</script> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CodeBlockAndContent5()
        {
            var typeAsString = ParseAndGenString(
@"
@{ 
  <b>something@(x)</b> 
} 
</html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
