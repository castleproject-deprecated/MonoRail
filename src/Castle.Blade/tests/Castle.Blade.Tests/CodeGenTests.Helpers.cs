namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void HelperDecl1()
        {
            var typeAsString = ParseAndGenString(
@"
@helper Name () { 
    <text>hello</text>
}
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void HelperDecl2()
        {
            var typeAsString = ParseAndGenString(
@"
@helper ExpXml (string xml, int depth) { 
    <text>hello</text> @xml
}
<html>
    @ExpXml(""<testing>"", 1)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

    }
}
