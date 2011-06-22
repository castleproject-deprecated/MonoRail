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
"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent2()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something</text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent21()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text></text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }

        [Test]
        public void IfBlockAndContent22()
        {
            var typeAsString = ParseAndGenString(
@"@if(x == 10) { <text>something<b>with</b></text>  }  </html>");
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
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
</html>"
);
            System.Diagnostics.Debug.WriteLine(typeAsString);
        }
    }
}
