// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.Framework.Tests.ViewComponents
{
	using System;
	using Framework.ViewComponents;
	using NUnit.Framework;

	[TestFixture]
	public class CombineJSViewComponentTestCase
	{
		[Test]
		public void DontResolveForNonRelative()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", ".icon { background-image: url(/myapp/test.png) }");
			Assert.AreEqual(".icon { background-image: url(/myapp/test.png) }", output);
		}

		[Test]
		public void ResolveForRelativeUrlsWithoutQuotes()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", @".icon { background-image: url(../images/test.png) }");
			Assert.AreEqual(".icon { background-image: url(/myapp/images/test.png) }", output);
		}

		[Test]
		public void ResolveForMultipleUrlsRelative()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", @".icon1 { background-image: url(../images/test1.png) }, .icon2 { background-image: url(../images/icons/test2.png) }");
			Assert.AreEqual(".icon1 { background-image: url(/myapp/images/test1.png) }, .icon2 { background-image: url(/myapp/images/icons/test2.png) }", output);
		}

		[Test]
		public void ResolveForRelativeUrlsWithSingleQuotes()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", @".icon1 { background-image: url('../images/icons/file-xslx.gif') !important; }, .icon2 { background-image: url('../images/icons/test2.png') }");
			Assert.AreEqual(@".icon1 { background-image: url('/myapp/images/icons/file-xslx.gif') !important; }, .icon2 { background-image: url('/myapp/images/icons/test2.png') }", output);
		}

		[Test]
		public void ResolveForRelativeUrlsWithDoubleQuotes()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", @".icon1 { background-image: url(""../images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url(""../images/icons/test2.png"") }");
			Assert.AreEqual(@".icon1 { background-image: url(""/myapp/images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url(""/myapp/images/icons/test2.png"") }", output);
		}

		[Test]
		public void DontResolveForNonRelativeUrlsWithSingleQuotes()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url('/images/icons/file-xslx.gif') !important; }, .icon2 { background-image: url('/images/icons/test2.png') }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}

		[Test]
		public void DontResolveForNonRelativeUrlsWithDoubleQuotes()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url(""/images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url(""/images/icons/test2.png"") }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}

		[Test]
		public void DontResolveForAbsoluteUrls()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url(""http://www.castleproject.org/images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url('http://www.castleproject.org//images/icons/test2.png') }, .icon3 { background-image: url(http://www.castleproject.org//images/icons/test3.png) }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}

		[Test]
		public void DontResolveForAbsoluteHttpsUrls()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url(""https://www.castleproject.org/images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url('https://www.castleproject.org//images/icons/test2.png') }, .icon3 { background-image: url(https://www.castleproject.org//images/icons/test3.png) }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}

		[Test]
		public void DontResolveForAbsoluteUrlsWithPortSpecified()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url(""http://www.castleproject.org:8080/images/icons/file-xslx.gif"") !important; }, .icon2 { background-image: url('http://www.castleproject.org:8080/images/icons/test2.png') }, .icon3 { background-image: url(http://www.castleproject.org:8080/images/icons/test3.png) }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}

		[Test]
		public void DontResolveForAbsoluteUrlsWithDataProtocol()
		{
			var resolver = new CombineJSViewComponent.CssRelativeUrlResolver(@"c:\inetpub\wwwroot\myapp\", new Uri("/myapp/", UriKind.Relative));
			var css = @".icon1 { background-image: url(""data:image/gif;base64,AAAA"") !important; }, .icon2 { background-image: url('data:image/gif;base64,AAAA') }, .icon3 { background-image: url(data:image/gif;base64,AAAA) }";
			var output = resolver.Resolve(@"c:\inetpub\wwwroot\myapp\css\main.css", css);
			Assert.AreEqual(css, output);
		}
	}
}