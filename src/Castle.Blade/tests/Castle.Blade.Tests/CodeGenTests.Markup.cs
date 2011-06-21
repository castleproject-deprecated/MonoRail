namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void MarkupWithSelfTerminatingElement()
        {
            var content =
@"@{ 
	<link src=""@x"" />
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void MarkupWithSelfTerminatingElement_NoTransition()
        {
            var content =
@"@{ 
	<link src=""this is a source"" rel=""something"" />
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ContentWithAtAt()
        {
            var content =
@"@@modeltype
@{
    Layout = ""~/Views/Shared/default.cshtml"";
}

<div class=""container"">
    <div class=""header"">
        <h1>Default Home Page - </h1>
        <h3>Well @name, there's nothing on earth like a genuine, bona fide, Electrified, Six-car Monorail!</h3>
    </div>

    @using (Form.BeginForm(WebApplication1.Controllers.TodoController.Urls.Create()))
    {
        <fieldset id=""contactForm"">
            <legend>Message</legend>
            <p>
                @Html.Label(""Email"", ""Email""): 
                @Html.TextInput(""Email"")
            </p>
            <p>
                <input type=""submit"" value=""Send"" />
            </p>
        </fieldset>
    } 
</div>";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
