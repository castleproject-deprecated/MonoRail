namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void ForeachAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"
@foreach(var x in list) 
{ 
    <b>@x</b>
}  

"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ForAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"
@for(var i = 0; i < 100; i++) 
{ 
    <b>@i</b>
}  

"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
