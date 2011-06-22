namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void BlockWithNamedLambdaParam1()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form(  @=> p <b>something @p</b> ); } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam2()
        {
            var typeAsString = ParseAndGenString("@{ Helper.Form(  @=> p { <b>something @p</b> } ); } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam3()
        {
            var typeAsString = ParseAndGenString("@Helper.Form(WebApplication1.Controllers.TodoController.Urls.Create(),  @=> p <fieldset id='tt'>something @p</fieldset> )  ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
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

})"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithNamedLambdaParam5()
        {
            var typeAsString = ParseAndGenString(
@"
@Helper.FormFor( @=> p {
    <fieldset id='tt'>something</fieldset> 
})"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockAssignment1()
        {
            var typeAsString = ParseAndGenString(
@"@{ 
var = @<b>something</b>;
} ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda1()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b>something</b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda2()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda3()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something@x</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambda4()
        {
            var typeAsString = ParseAndGenString(
@"@{ Helper.Form( @<b><a>something@(x)</a></b> ) } ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void CallWithLambda1()
        {
            var typeAsString = ParseAndGenString(
@"@Helper.Form( @<b><a>something@(x)</a></b> )  ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void BlockWithLambdaAssignment()
        {
            var typeAsString = ParseAndGenString(
@"@{ 
    var x = @<b><a>something</a></b>; 
} ");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
