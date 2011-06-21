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
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
