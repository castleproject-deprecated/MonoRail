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

namespace Castle.MonoRail.Framework.Tests.Routing
{
	using Framework.Routing;
	using NUnit.Framework;

	[TestFixture]
	public class PatternRouteMatchTestCase : BaseRuleTestFixture
	{
		[Test]
		public void ShouldMatchSimplesSlash()
		{
			var route = new PatternRoute("/");
			var match = new RouteMatch();
			Assert.AreEqual(1, route.Matches("/", CreateGetContext(), match));
			Assert.AreEqual(1, route.Matches("", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("some", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("/some", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchStatic()
		{
			var route = new PatternRoute("/some/path");
			var match = new RouteMatch();
			Assert.AreEqual(8000, route.Matches("/some/path", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchStaticWithFileExtension()
		{
			var route = new PatternRoute("/default.aspx");
			var match = new RouteMatch();
			Assert.AreEqual(8000, route.Matches("/default.aspx", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchStaticCaseInsensitive()
		{
			var route = new PatternRoute("/default.aspx");
			var match = new RouteMatch();
			Assert.AreEqual(8000, route.Matches("/DEFAULT.ASPX", CreateGetContext(), match));

			route = new PatternRoute("/some/path");
			match = new RouteMatch();
			Assert.AreEqual(8000, route.Matches("/SOME/Path", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchHiphensAndUnderlines()
		{
			var route = new PatternRoute("/some/path_to-this");
			var match = new RouteMatch();
			Assert.AreEqual(8000, route.Matches("/some/path_to-this", CreateGetContext(), match)); 
		}

		[Test]
		public void NamedRequiredParameters()
		{
			var route = new PatternRoute("/<controller>/<action>");
			var match = new RouteMatch();
			Assert.AreEqual(4000, route.Matches("/some/act", CreateGetContext(), match));
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act", match.Parameters["action"]);
		}

		[Test]
		public void NamedRequiredParametersForExtension()
		{
			var route = new PatternRoute("/<controller>/<action>.<format>");
			var match = new RouteMatch();
			Assert.AreEqual(6000, route.Matches("/some/act.xml", CreateGetContext(), match));
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act", match.Parameters["action"]);
			Assert.AreEqual("xml", match.Parameters["format"]);
		}

		[Test]
		public void NamedParametersCanHaveUnderlines()
		{
			var route = new PatternRoute("/<controller>/<action>");
			var match = new RouteMatch();
			route.Matches("/some/act_name", CreateGetContext(), match);
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act_name", match.Parameters["action"]);
		}

		[Test]
		public void NamedParametersCanHaveHiphens()
		{
			var route = new PatternRoute("/<controller>/<action>");
			var match = new RouteMatch();
			route.Matches("/some/act-name", CreateGetContext(), match);
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act-name", match.Parameters["action"]);
		}

		[Test]
		public void NamedParametersCanHaveSpaces()
		{
			var route = new PatternRoute("/<controller>/<action>");
			var match = new RouteMatch();
			route.Matches("/some/act name", CreateGetContext(), match);
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act name", match.Parameters["action"]);
		}

		[Test]
		public void NamedOptionalParameters()
		{
			var route = new PatternRoute("/<controller>/[action]/[id]");
			var match = new RouteMatch();
			Assert.AreEqual(4001, route.Matches("/some/act", CreateGetContext(), match));
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act", match.Parameters["action"]);

			match = new RouteMatch();
			Assert.AreEqual(6000, route.Matches("/some/act/10", CreateGetContext(), match));
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("act", match.Parameters["action"]);
			Assert.AreEqual("10", match.Parameters["id"]);
		}

		[Test]
		public void NamedOptionalParametersWithDefaults()
		{
			var route = new PatternRoute("/<controller>/[action]/[id]")
				.DefaultFor("action").Is("index").DefaultFor("id").Is("0");
			var match = new RouteMatch();
			Assert.AreEqual(2002, route.Matches("/some", CreateGetContext(), match));
			Assert.AreEqual("some", match.Parameters["controller"]);
			Assert.AreEqual("index", match.Parameters["action"]);
			Assert.AreEqual("0", match.Parameters["id"]);
		}

		[Test]
		public void NamedOptionalParametersWithRestrictions()
		{
			var route = new PatternRoute("/<controller>/[action]/[id]")
				.Restrict("action").AnyOf("index", "list")
				.Restrict("id").ValidInteger;

			var match = new RouteMatch();
			Assert.AreEqual(4001, route.Matches("/some/index", CreateGetContext(), match));
			Assert.AreEqual(4001, route.Matches("/some/list", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("/some/new", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("/some/index/foo", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("/some/list/bar", CreateGetContext(), match));
			Assert.AreEqual(6000, route.Matches("/some/list/1", CreateGetContext(), match));
		}

		[Test]
		public void NamedRequiredParametersWithRestrictions()
		{
			var matchGuid = 
				"[A-Fa-f0-9]{32}|" +
				"({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?|" +
				"({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})";

			var route = new PatternRoute("/<param>/<key>")
				.Restrict("key").ValidRegex(matchGuid);

			var match = new RouteMatch();
			Assert.AreEqual(0, route.Matches("/something/zzzzzzzz-c123-11dc-95ff-0800200c9a66", CreateGetContext(), match));
			Assert.AreEqual(4000, route.Matches("/something/173e0970-c123-11dc-95ff-0800200c9a66", CreateGetContext(), match));
			Assert.AreEqual("something", match.Parameters["param"]);
			Assert.AreEqual("173e0970-c123-11dc-95ff-0800200c9a66", match.Parameters["key"]);
		}

		[Test]
		public void AnythingBut_Restriction()
		{
			var route = new PatternRoute("/<controller>/[action]/[id]")
				.Restrict("controller").AnythingBut("dummy")
				.Restrict("id").ValidInteger;

			var match = new RouteMatch();
			Assert.AreEqual(0, route.Matches("/dummy/index", CreateGetContext(), match));
			Assert.AreEqual(0, route.Matches("/DUMMY/list", CreateGetContext(), match));
			Assert.AreEqual(4001, route.Matches("/some/new", CreateGetContext(), match));
			Assert.AreEqual(6000, route.Matches("/some/list/1", CreateGetContext(), match));
		}

		[Test]
		public void ShouldReturnNonZeroForMatchedDefaults()
		{
			var route = new PatternRoute("/[controller]/[action]");

			var match = new RouteMatch();
			Assert.AreEqual(2, route.Matches("/", CreateGetContext(), match));
			Assert.IsTrue(match.Parameters.ContainsKey("controller"));
			Assert.IsTrue(match.Parameters.ContainsKey("action"));
		}

		[Test]
		public void ShouldMatchEmptyUrl()
		{
			var route = new PatternRoute("/[controller]/[action]");

			var match = new RouteMatch();
			Assert.AreEqual(2, route.Matches("", CreateGetContext(), match));
			Assert.IsTrue(match.Parameters.ContainsKey("controller"));
			Assert.IsTrue(match.Parameters.ContainsKey("action"));
		}

		[Test]
		public void ShouldReturnZeroForMissingRequiredPart()
		{
			var route = new PatternRoute("/<controller>/[action]");

			var match = new RouteMatch();
			Assert.AreEqual(0, route.Matches("/", CreateGetContext(), match));
		}

		[Test]
		public void ShouldNotMatchWhenRestrictingVerbs()
		{
			var route = new PatternRoute("/simple")
				.RestrictTo(Verb.Post);

			var match = new RouteMatch();
			Assert.AreEqual(0, route.Matches("/simple", CreateGetContext(), match));
		}

		[Test]
		public void ShouldNotMatchWhenRestrictingMultipleVerbs()
		{
			var route = new PatternRoute("/simple")
				.RestrictTo(Verb.Post | Verb.Put);

			var match = new RouteMatch();
			Assert.AreEqual(0, route.Matches("/simple", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchWhenRestrictingWithMatchingVerb()
		{
			var route = new PatternRoute("/simple")
				.RestrictTo(Verb.Get);

			var match = new RouteMatch();
			Assert.Less(0, route.Matches("/simple", CreateGetContext(), match));
		}

		[Test]
		public void ShouldMatchWhenRestrictingWithMatchingMultipleVerb()
		{
			var route = new PatternRoute("/simple")
				.RestrictTo(Verb.Get | Verb.Post);

			var match = new RouteMatch();
			Assert.Less(0, route.Matches("/simple", CreateGetContext(), match));
		}
	}
}
