namespace Castle.Blade.Tests
{
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void ForeachAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"@foreach(var x in list) 
{ 
    <b>@x</b>
}");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { foreach ( var x in list ) { 
    WriteLiteral ( ""<b>"" ) ; 
    Write ( x ) ; 
    WriteLiteral ( ""</b>"" ) ; } } } } ", normalizedCode);
        }

        [Test]
        public void ForAndContent1()
        {
            var typeAsString = ParseAndGenString(
@"@for(var i = 0; i < 100; i++) 
{ 
    <b>@i</b>
}");
            var normalizedCode = Normalize(typeAsString);
            // DebugWrite(normalizedCode);

            Assert.AreEqual(
@"namespace Blade { public class Generated_Type : Castle . Blade . BaseBladePage { public override void RenderPage ( ) { for ( var i = 0 ; i < 100 ; i ++ ) { 
    WriteLiteral ( ""<b>"" ) ; 
    Write ( i ) ; 
    WriteLiteral ( ""</b>"" ) ; } } } } ", normalizedCode);
        }
    }
}
