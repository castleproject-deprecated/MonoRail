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
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup2()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string>();
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup3()
        {
            var content =
@"@{ 
	var parameters = new Dictionary<string, object>(); <p>sss</p>
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void GenericIsNotTakenAsMarkup4()
        {
            var content =
@"@{ 
	<p>sss</p> var parameters = new Dictionary<string, object>();
}";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
";
            var typeAsString = ParseAndGenString(content);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
