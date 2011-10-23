namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void GenericIsNotTakenAsMarkup1()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string, object>();
}";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage " + 
@"{ public override void RenderPage ( ) " + 
@"{ var parameters = new Dictionary < string , object > ( ) ; } } } ", normalizedCode);
        }

        

        [Test]
        public void GenericIsNotTakenAsMarkup2()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string>();
}";
            var typeAsString = ParseAndGenString(content);
            var normalizedCode = Normalize(typeAsString);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage " +
@"{ public override void RenderPage ( ) " +
@"{ var parameters = new Dictionary < string > ( ) ; } } } ", normalizedCode);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup3()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string, object>(); <p>sss</p>
}";
            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage " +
@"{ public override void RenderPage ( ) " +
@"{ var parameters = new Dictionary < string , object > ( ) ; 
    WriteLiteral ( ""<p>"" ) ; 
    WriteLiteral ( ""sss</p>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup4()
        {
            var content =
@"@{ 
	<p>sss</p> var parameters = new Dictionary<string, object>();
}";
            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage " +
@"{ public override void RenderPage ( ) " +
@"{ 
    WriteLiteral ( ""<p>"" ) ; 
    WriteLiteral ( ""sss</p>"" ) ; var parameters = new Dictionary < string , object > ( ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IdentifiersAreKeptIsolated()
        {
            var content =
@"
@pageSize
@pageSize<
@pageSize!
@pageSize-
@pageSize}
@pageSize)
@pageSize.
@pageSize.<a>test<a>
@pageSize{
@pageSize[
@pageSize>
@pageSize,
@pageSize;
@pageSize:
@pageSize""
";
            var typeAsString = ParseAndGenString(content);
            // DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage " +
@"{ public override void RenderPage ( ) " +
@"{ 
    WriteLiteral ( ""\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""<\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""!\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""-\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""}\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "")\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "".\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "".<a>test<a>\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""{\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""[\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "">\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "",\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "";\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( "":\r\n"" ) ; 
    Write ( pageSize ) ; 
    WriteLiteral ( ""\""\r\n"" ) ; } } } ", normalizedCode);
        }
    }
}
