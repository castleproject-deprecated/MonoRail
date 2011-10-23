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
    @Name()
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public HtmlResult Name ( ) { return new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""hello"" ) ; } ) ; } " + 
  @"public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""\r\n<html>\r\n    "" ) ; 
    Write ( Name ( ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void HelperDecl2()
        {
            var typeAsString = ParseAndGenString(
@"
@helper ExpXml (string xml, int depth) { 
    <text>hello</text> 
    xml;
}
<html>
    @ExpXml(""<testing>"", 1)
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public HtmlResult ExpXml ( string xml , int depth ) { " + 
    @"return new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""hello"" ) ; xml ; } ) ; } public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; 
    WriteLiteral ( ""\r\n<html>\r\n    "" ) ; 
    Write ( ExpXml ( ""<testing>"" , 1 ) ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

    }
}
