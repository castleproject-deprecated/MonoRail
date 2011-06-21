namespace Castle.Blade.Tests
{
    using System;
    using NUnit.Framework;

    public partial class CodeGenTests
    {
        [Test]
        public void InheritsStmt1()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits My.BaseClass;
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void InheritsStmt2()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits My.BaseClass
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void InheritsStmt_Fail()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void InheritsStmt_Fail2()
        {
            var typeAsString = ParseAndGenString(
@"
@inherits ;
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ImportNamespace1()
        {
            var typeAsString = ParseAndGenString(
@"
@using My.Namespace;
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ImportNamespace2()
        {
            var typeAsString = ParseAndGenString(
@"
@using My.Namespace
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void ImportNamespace3()
        {
            var typeAsString = ParseAndGenString(
@"
@using Namespace
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        // [Test, ExpectedException(typeof(Exception))]
        [Test]
        public void ImportNamespace_Invalid()
        {
            ParseAndGenString(
@"
@using ;
<html>
    @DoSomething(10)
</html>"
);
        }

        // [Test, ExpectedException(typeof(Exception), MatchType = MessageMatch.StartsWith, ExpectedMessage = "Expecting namespace")]
        [Test]
        public void ImportNamespace_Invalid2()
        {
            var typeAsString = ParseAndGenString(
@"
@using
<html>
    @DoSomething(10)
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
