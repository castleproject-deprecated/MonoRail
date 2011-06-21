namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void CodeBlockAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"<html> @{ LayoutName = ""test""; }");
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
    }
}
