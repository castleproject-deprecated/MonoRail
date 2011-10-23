namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
		[Test]
		public void BlockWithLambdaAndInlineTemplateWithOutput()
		{
			var typeAsString = ParseAndGenString("@( ViewComponent.Render<GridComponent>(c => c.Header = @<th class=\"line_title\" style=\"width: 200px\">Data</th> ) )");
		}

    	[Test]
        public void BlockWithNamedLambdaParam1()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form(  @=> p <b>something @p</b> ); } ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
@"Helper . Form ( p => new HtmlResult ( __writer1 => " +
@"{ 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""something "" ) ; 
    Write ( __writer1 , p ) ; 
    WriteLiteral ( @__writer1 , ""</b>"" ) ; } " +
@") ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithNamedLambdaParamInBraces()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form( @=> p { <b>something @p</b> } ); } ");
            DebugWrite(typeAsString);
            var normalizedCode = Normalize(typeAsString);
            DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " +
@"Helper . Form ( p => new HtmlResult ( __writer1 => " +
@"{ 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""something "" ) ; 
    Write ( __writer1 , p ) ; 
    WriteLiteral ( @__writer1 , ""</b>"" ) ; } " +
@") ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithNamedLambdaParam3()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.Form(WebApplication1.Controllers.TodoController.Urls.Create(), @=> p <fieldset id='tt'>something @p</fieldset> )");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " + 
@"public override void RenderPage ( ) { 
    Write ( Helper . Form ( WebApplication1 . Controllers . TodoController . Urls . Create ( ) , p => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<fieldset id='tt'>"" ) ; 
    WriteLiteral ( @__writer1 , ""something "" ) ; 
    Write ( __writer1 , p ) ; 
    WriteLiteral ( @__writer1 , ""</fieldset>"" ) ; } ) ) ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithNamedLambdaParam4()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.FormFor(WebApplication1.Controllers.TodoController.Urls.Create(),  @=> p 
{
    <fieldset id='tt'>something @p

    @p.FieldsFor( @=> p2 {
        <fieldset>
        @p2.FieldFor(""some"")
        </fieldset>
    })

    </fieldset> 

})");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    Write ( Helper . FormFor ( WebApplication1 . Controllers . TodoController . Urls . Create ( ) , p => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<fieldset id='tt'>"" ) ; 
    WriteLiteral ( @__writer1 , ""something "" ) ; 
    Write ( __writer1 , p ) ; 
    WriteLiteral ( @__writer1 , ""\r\n\r\n    "" ) ; 
    Write ( __writer1 , p . FieldsFor ( p2 => new HtmlResult ( __writer2 => { 
    WriteLiteral ( @__writer2 , ""<fieldset>"" ) ; 
    WriteLiteral ( @__writer2 , ""\r\n        "" ) ; 
    Write ( __writer2 , p2 . FieldFor ( ""some"" ) ) ; 
    WriteLiteral ( @__writer2 , ""\r\n        </fieldset>"" ) ; } ) ) ) ; 
    WriteLiteral ( @__writer1 , ""\r\n\r\n    </fieldset>"" ) ; } ) ) ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithNamedLambdaParam5()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.FormFor( @=> p {
    <fieldset id='tt'>something</fieldset> 
})");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { 
    Write ( Helper . FormFor ( p => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<fieldset id='tt'>"" ) ; 
    WriteLiteral ( @__writer1 , ""something</fieldset>"" ) ; } ) ) ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockAssignment1()
        {
            var typeAsString = ParseAndGenString(
@"@{ 
    var = @<b>something</b>;
} ");
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { var = item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""something</b>"" ) ; } ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithLambda1()
        {
            var typeAsString = ParseAndGenString(@"@{ Helper.Form( @<b>something</b> ); } ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " + 
@"Helper . Form ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""something</b>"" ) ; } ) ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithLambda2()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something</a></b> ); } ");
            var normalizedCode = Normalize(typeAsString);
            //DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " + 
@"Helper . Form ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""<a>something</a></b>"" ) ; } ) ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithLambdaAndTransition()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something@x</a></b> ); } ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade " +
@"{ public class Generated_Type : Castle . Blade . BaseBladePage { " +
@"public override void RenderPage ( ) { " + 
@"Helper . Form ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""<a>something"" ) ; 
    Write ( __writer1 , x ) ; 
    WriteLiteral ( @__writer1 , ""</a></b>"" ) ; } ) ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithLambda4()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something@(x)</a></b> ); } ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { " +
@"public class Generated_Type : Castle . Blade . BaseBladePage { " +
    @"public override void RenderPage ( ) { " + 
        @"Helper . Form ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""<a>something"" ) ; 
    Write ( __writer1 , x ) ; 
    WriteLiteral ( @__writer1 , ""</a></b>"" ) ; } ) ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void CallWithLambda1()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.Form( @<b><a>something@(x)</a></b> )  ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { " +
    @"public class Generated_Type : Castle . Blade . BaseBladePage { " + 
        @"public override void RenderPage ( ) { 
    Write ( Helper . Form ( item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""<a>something"" ) ; 
    Write ( __writer1 , x ) ; 
    WriteLiteral ( @__writer1 , ""</a></b>"" ) ; } ) ) ) ; 
    WriteLiteral ( ""  "" ) ; } } } ", normalizedCode);
        }

        [Test]
        public void BlockWithLambdaAssignment()
        {
            var typeAsString = ParseAndGenString(
@"@{ 
    var x = @<b><a>something</a></b>; 
} ");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { " +
    @"public class Generated_Type : Castle . Blade . BaseBladePage { " +
        @"public override void RenderPage ( ) { " + 
            @"var x = item => new HtmlResult ( __writer1 => { 
    WriteLiteral ( @__writer1 , ""<b>"" ) ; 
    WriteLiteral ( @__writer1 , ""<a>something</a></b>"" ) ; } ) ; 
    WriteLiteral ( "" "" ) ; } } } ", normalizedCode);
        }
    }
}
