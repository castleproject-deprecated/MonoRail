namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void FunctionsBlock()
        {
            var typeAsString = ParseAndGenString(
@"<html>
@functions {
        string DoSomething(int x) { 
            return ""aa"";
        }
}
    @DoSomething(10)
    @DoSomething(20)

</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
