namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void IfBlockAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) 
{ 
    @:text 
}  
</html> 
");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { " + 
    @"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""text "" ) ; } 
    WriteLiteral ( ""  \r\n</html> \r\n"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent2()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something</text>  }  </html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);
            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
@"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""something"" ) ; " +
@"} 
    WriteLiteral ( ""  </html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent21()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text></text>  }  </html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
@"if ( ( x == 10 ) ) { } 
    WriteLiteral ( ""  </html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent22()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something<b>with</b></text>  }  </html>");
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);
            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
    @"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""something<b>with</b>"" ) ; " +
@"} 
    WriteLiteral ( ""  </html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent23()
        {
            var typeAsString = ParseAndGenString(
@"
@if(x == 10) 
{ 
    <text>something<b>@x</b></text>  
}  
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);
            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; " +
@"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""something<b>"" ) ; 
    Write ( x ) ; 
    WriteLiteral ( ""</b>"" ) ; " +
@"} 
    WriteLiteral ( ""  \r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent3()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { 
    if (x++ == 11) 
    { 
        call();
    } 
    <text>something</text> 
    Debug.WriteLine(""some""); 
} 
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);
            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
@"if ( ( x == 10 ) ) { " +
    @"if ( x ++ == 11 ) { " +
        @"call ( ) ; } 
    WriteLiteral ( ""something"" ) ; Debug . WriteLine ( ""some"" ) ; } 
    WriteLiteral ( "" \r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent4()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { 
    <text>something</text> 
} else { 
    <text>else</text> 
}
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
    @"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""something"" ) ; } " +
@"else { 
    WriteLiteral ( ""else"" ) ; " +
@"} 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void IfBlockAndContent5()
        {
            var typeAsString = ParseAndGenString(
@"
@if(x == 10) { 
    <text>something</text> 
} else if (x == 20) { 
    <text>else</text> 
}
</html>");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    WriteLiteral ( ""\r\n"" ) ; " +
@"if ( ( x == 10 ) ) { 
    WriteLiteral ( ""something"" ) ; } " +
@"else { if ( ( x == 20 ) ) { 
    WriteLiteral ( ""else"" ) ; } " +
@"} 
    WriteLiteral ( ""\r\n</html>"" ) ; } } } ", normalizedCode);
        }
    }
}
