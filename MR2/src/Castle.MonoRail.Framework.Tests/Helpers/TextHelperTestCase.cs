// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Tests.Helpers
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.Threading;
	using Castle.MonoRail.Framework.Helpers;
	using NUnit.Framework;

	[TestFixture]
	public class TextHelperTestCase
	{
		private static TextHelper helper = new TextHelper();

		[Test]
		public void FormatPhone_CanFormatForUSCulture()
		{
			var en = CultureInfo.CreateSpecificCulture("en");
			Thread.CurrentThread.CurrentCulture = en;
			Thread.CurrentThread.CurrentUICulture = en;

			Assert.AreEqual("(123) 456-1234", helper.FormatPhone("1234561234"));
		}

		[Test]
		public void FormatPhone_IgnoresStringTooSmall()
		{
			var en = CultureInfo.CreateSpecificCulture("en");
			Thread.CurrentThread.CurrentCulture = en;
			Thread.CurrentThread.CurrentUICulture = en;

			Assert.AreEqual("123", helper.FormatPhone("123"));
		}

		[Test]
		public void FormatPhone_IgnoresStringIfSomeNonNumericCharsArePresent()
		{
			var en = CultureInfo.CreateSpecificCulture("en");
			Thread.CurrentThread.CurrentCulture = en;
			Thread.CurrentThread.CurrentUICulture = en;

			Assert.AreEqual("123-111-1212", helper.FormatPhone("123-111-1212"));
			Assert.AreEqual("(123)111-1212", helper.FormatPhone("(123)111-1212"));
			Assert.AreEqual("123.111.1212", helper.FormatPhone("123.111.1212"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PascalCaseToWord_CannotAcceptNulls()
		{
			TextHelper.PascalCaseToWord(null);
		}

		[Test]
		public void PascalCaseToWord_SeparatesWordsBasedOnCase()
		{
			Assert.AreEqual("Sequence Info", TextHelper.PascalCaseToWord("SequenceInfo"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ToSentenceWithNull()
		{
			var sentence = TextHelper.ToSentence(null);
			Assert.AreEqual("", sentence);
		}

		[Test]
		public void ToSentenceWithStringArrayNoElements()
		{
			var sentence = TextHelper.ToSentence(new string[0]);
			Assert.AreEqual("", sentence);
		}

		[Test]
		public void ToSentenceWithStringArrayOneElement()
		{
			var sentence = TextHelper.ToSentence(new[] { "apple" });
			Assert.AreEqual("apple", sentence);
		}

		[Test]
		public void ToSentenceWithStringArrayTwoElements()
		{
			var sentence = TextHelper.ToSentence(new[] { "apple", "banana" });
			Assert.AreEqual("apple and banana", sentence);
		}

		[Test]
		public void ToSentenceWithStringArrayThreeElements()
		{
			var sentence = TextHelper.ToSentence(new[] { "apple", "banana", "mango" });
			Assert.AreEqual("apple, banana and mango", sentence);
		}

		[Test]
		public void ToSentenceWithSpecifiedConnector()
		{
			var sentence = TextHelper.ToSentence(new[] { "apple", "banana", "mango" }, "y");
			Assert.AreEqual("apple, banana y mango", sentence);
		}

		[Test]
		public void ToSentenceWithCommaBeforeConnectorSpecified()
		{
			var sentence = TextHelper.ToSentence(new[] { "apple", "banana", "mango" }, TextHelper.DefaultConnector, false);
			Assert.AreEqual("apple, banana, and mango", sentence);
		}

		private class Person
		{
			private readonly string _firstName;
			private readonly string _lastName;

			public Person(string firstName, string lastName)
			{
				_firstName = firstName;
				_lastName = lastName;
			}

			public override string ToString()
			{
				return _firstName + " " + _lastName;
			}
		}

		[Test]
		public void ToSentenceWithArrayListOfPeople()
		{
			var people = new ArrayList
			{
				new Person("Clark", "Kent"),
				new Person("Lois", "Lane"),
				new Person("Lex", "Luther")
			};
			var sentence = TextHelper.ToSentence(people);
			Assert.AreEqual("Clark Kent, Lois Lane and Lex Luther", sentence);
		}
	}
}