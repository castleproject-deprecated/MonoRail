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
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"string DoSomething ( int x ) { return ""aa"" ; } " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""<html>\r\n"" ) ; 
    WriteLiteral ( ""\r\n    "" ) ; 
    Write ( DoSomething ( 10 ) ) ; 
    WriteLiteral ( ""\r\n    "" ) ; 
    Write ( DoSomething ( 20 ) ) ; 
    WriteLiteral ( ""\r\n\r\n</html>"" ) ; } } } ", normalizedCode);
        }
    }
}
