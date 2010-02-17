// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.AspView.Tests.Utilities
{
	using System.Linq;
	using Internal;
	using NUnit.Framework;

	[TestFixture]
	public class ViewPropertiesDictionaryTests
	{
		const int Range = 20;
		readonly string[] keys = Enumerable.Range(1, Range).Select(i => "key" + i).ToArray();
		readonly int[] values = Enumerable.Range(1, Range).ToArray();
		
		
		[Test]
		public void SingleScope_Sanity()
		{
			var dict = new ViewPropertiesDictionary();

			dict[keys[0]] = values[0];
			dict[keys[1]] = values[1];

			Assert.That(dict[keys[0]], Is.EqualTo(values[0]));
			Assert.That(dict[keys[1]], Is.EqualTo(values[1]));
		}

		[Test]
		public void TwoScopes_BothScopesAreVisible()
		{
			var parent = new ParametersDictionary();
			parent[keys[0]] = values[0];

			var dict = new ViewPropertiesDictionary(parent);
			dict[keys[1]] = values[1];

			Assert.That(dict[keys[0]], Is.EqualTo(values[0]));
			Assert.That(dict[keys[1]], Is.EqualTo(values[1]));
		}

		[Test]
		public void TwoScopes_LocalScopeHideParentScope()
		{
			var parent = new ParametersDictionary();
			parent[keys[0]] = values[0];

			var dict = new ViewPropertiesDictionary(parent);
			dict[keys[0]] = values[1];

			Assert.That(dict[keys[0]], Is.EqualTo(values[1]));
		}

		[Test]
		public void TwoScopes_WhenLocalScopeHides_ParentScopeDoesNotChange()
		{
			var parent = new ParametersDictionary();
			parent[keys[0]] = values[0];

			var dict = new ViewPropertiesDictionary(parent);
			dict[keys[0]] = values[1];

			Assert.That(parent[keys[0]], Is.EqualTo(values[0]));
		}

		[Test]
		public void MultipleScopes_WhenLocalScopeHides_ParentScopeDoesNotChange()
		{
			var level0 = new ParametersDictionary();

			level0[keys[0]] = "key0_level0";
			level0[keys[1]] = "key1_level0";
			level0[keys[2]] = "key2_level0";
			level0[keys[3]] = "key3_level0";

			var level1 = new ViewPropertiesDictionary(level0);
			level1[keys[0]] = "key0_level1";
			level1[keys[4]] = "key4_level1";
			level1[keys[5]] = "key5_level1";
			level1[keys[6]] = "key6_level1";

            var level2 = new ViewPropertiesDictionary(level1);
			level2[keys[0]] = "key0_level2";
			level2[keys[1]] = "key1_level2";
			level2[keys[4]] = "key4_level2";
			level2[keys[7]] = "key7_level2";
			level2[keys[8]] = "key8_level2";

			var level3 = new ViewPropertiesDictionary(level2);
			level3[keys[0]] = "key0_level3";
			level3[keys[1]] = "key1_level3";
			level3[keys[2]] = "key2_level3";
			level3[keys[4]] = "key4_level3";
			level3[keys[7]] = "key7_level3";
			level3[keys[9]] = "key9_level3";


			// Level 0 
			Assert.That(level0[keys[0]], Is.EqualTo("key0_level0"));
			Assert.That(level0[keys[1]], Is.EqualTo("key1_level0"));
			Assert.That(level0[keys[2]], Is.EqualTo("key2_level0"));
			Assert.That(level0[keys[3]], Is.EqualTo("key3_level0"));
			foreach (var i in Enumerable.Range(4, Range-4))
			{
				Assert.That(level0.Contains(keys[i]), Is.Not.True);
			}

			// Level 1 
			Assert.That(level1[keys[0]], Is.EqualTo("key0_level1"));
			Assert.That(level1[keys[1]], Is.EqualTo("key1_level0"));
			Assert.That(level1[keys[2]], Is.EqualTo("key2_level0"));
			Assert.That(level1[keys[3]], Is.EqualTo("key3_level0"));
			Assert.That(level1[keys[4]], Is.EqualTo("key4_level1"));
			Assert.That(level1[keys[5]], Is.EqualTo("key5_level1"));
			Assert.That(level1[keys[6]], Is.EqualTo("key6_level1"));
			foreach (var i in Enumerable.Range(7, Range - 7))
			{
				Assert.That(level1.Contains(keys[i]), Is.Not.True);
			}

			// Level 2
			Assert.That(level2[keys[0]], Is.EqualTo("key0_level2"));
			Assert.That(level2[keys[1]], Is.EqualTo("key1_level2"));
			Assert.That(level2[keys[2]], Is.EqualTo("key2_level0"));
			Assert.That(level2[keys[3]], Is.EqualTo("key3_level0"));
			Assert.That(level2[keys[4]], Is.EqualTo("key4_level2"));
			Assert.That(level2[keys[5]], Is.EqualTo("key5_level1"));
			Assert.That(level2[keys[6]], Is.EqualTo("key6_level1"));
			Assert.That(level2[keys[7]], Is.EqualTo("key7_level2"));
			Assert.That(level2[keys[8]], Is.EqualTo("key8_level2"));
			foreach (var i in Enumerable.Range(9, Range - 9))
			{
				Assert.That(level2.Contains(keys[i]), Is.Not.True);
			}

			// Level 3
			Assert.That(level3[keys[0]], Is.EqualTo("key0_level3"));
			Assert.That(level3[keys[1]], Is.EqualTo("key1_level3"));
			Assert.That(level3[keys[2]], Is.EqualTo("key2_level3"));
			Assert.That(level3[keys[3]], Is.EqualTo("key3_level0"));
			Assert.That(level3[keys[4]], Is.EqualTo("key4_level3"));
			Assert.That(level3[keys[5]], Is.EqualTo("key5_level1"));
			Assert.That(level3[keys[6]], Is.EqualTo("key6_level1"));
			Assert.That(level3[keys[7]], Is.EqualTo("key7_level3"));
			Assert.That(level3[keys[8]], Is.EqualTo("key8_level2"));
			Assert.That(level3[keys[9]], Is.EqualTo("key9_level3"));
			foreach (var i in Enumerable.Range(10, Range - 10))
			{
				Assert.That(level3.Contains(keys[i]), Is.Not.True);
			}
		}
	}
}