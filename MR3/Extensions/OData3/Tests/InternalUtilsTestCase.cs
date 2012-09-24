using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests
{
	[TestFixture]
	public class InternalUtilsTestCase
	{
		[TestCase(typeof(IEnumerable<string>), typeof(string))]
		[TestCase(typeof(IEnumerable<InternalUtilsTestCase>), typeof(InternalUtilsTestCase))]
		[TestCase(typeof(List<string>), typeof(string))]
		[TestCase(typeof(IList<string>), typeof(string))]
		[TestCase(typeof(string[]), typeof(string))]
		[TestCase(typeof(List<InternalUtilsTestCase>), typeof(InternalUtilsTestCase))]
		public void getEnumerableElementType_for_valid_collection_types(Type input, Type expectedElement)
		{
			var option = InternalUtils.getEnumerableElementType(input);
			Assert.NotNull(option);
			Assert.AreSame(expectedElement, option.Value);
		}

		[TestCase(typeof(string))]
		[TestCase(typeof(Int32))]
		[TestCase(typeof(InternalUtilsTestCase))]
		public void getEnumerableElementType_for_ordinary_types(Type input)
		{
			var option = InternalUtils.getEnumerableElementType(input);
			Assert.Null(option);
		}
		
		
	}
}
