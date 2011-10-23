namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void ContentOnlySection()
        {
            var content =
@"

@section AdditionalStyles
{
    .ui-corner-all { -moz-border-radius: 4px; -webkit-border-radius: 4px; border-radius: 4px; }
}

";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n\r\n"" ) ; " + 
@"this . DefineSection ( ""AdditionalStyles"" , ( __writer1 ) => { 
    WriteLiteral ( @__writer1 , ""\r\n    .ui-corner-all { -moz-border-radius: 4px; -webkit-border-radius: 4px; border-radius: 4px; }\r\n"" ) ; } ) ; 
    WriteLiteral ( ""\r\n\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void TransitionsWithinQuotes()
        {
            var content =
                @"
@section AdditionalHeader
{
    <!--[if lte IE 8]><script language=""javascript"" type=""text/javascript"" src=""@Url.Content(""/Content/Scripts/excanvas/excanvas.min.js"")""></script><![endif]-->
    <script src=""@Url.Content(""/Content/Scripts/json2.js"")""></script>  
}

<html />
";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; " + 
@"this . DefineSection ( ""AdditionalHeader"" , ( __writer1 ) => { 
    WriteLiteral ( @__writer1 , ""\r\n    <!--[if lte IE 8]><script language=\""javascript\"" type=\""text/javascript\"" src=\"""" ) ; 
    Write ( __writer1 , Url . Content ( ""/Content/Scripts/excanvas/excanvas.min.js"" ) ) ; 
    WriteLiteral ( @__writer1 , ""\""></script><![endif]-->\r\n    <script src=\"""" ) ; 
    Write ( __writer1 , Url . Content ( ""/Content/Scripts/json2.js"" ) ) ; 
    WriteLiteral ( @__writer1 , ""\""></script>  \r\n"" ) ; } ) ; 
    WriteLiteral ( ""\r\n\r\n<html />\r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void Section_Empty()
        {
            var typeAsString = ParseAndGenString(
@"<html>
@section Name
{
    <somecontent> here </somecontent>
}
</html>"
);
            var normalizedCode = Normalize(typeAsString);
            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""<html>\r\n"" ) ; " + 
@"this . DefineSection ( ""Name"" , ( __writer1 ) => { 
    WriteLiteral ( @__writer1 , ""\r\n    <somecontent> here </somecontent>\r\n"" ) ; } ) ; 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }
    }
}
