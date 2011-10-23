namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void ContentWithTransition1()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@x</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition2()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@(x)</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition3()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@Helper.Form()</b>");

            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransition4()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@Helper.Form( x, 10, ""test"", '1' )</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithTransitionCodeBlock1()
        {
            var typeAsString = ParseAndGenString(
                @"<b>@{ Helper.Form( x, 10, ""test"", '1' ) }</b>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
