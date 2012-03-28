namespace Castle.MonoRail.Tests.Internal
{
	using System;
	using System.Globalization;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class ConversionTestCase
	{
		[SetUp]
		public void Establish_Context()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("pt-BR");
		}

		[TestCase("R$ 100", 100)]
		[TestCase("R$ 100,50", 100.5)]
		[TestCase("R$ 0,50", 0.5)]
		[TestCase("200", 200)]
		[TestCase("200,50", 200.5)]
		[TestCase("0,50", 0.5)]
		public void Convert_should_transform_input_to_decimal_based_on_the_current_ci(string input, double expected)
		{
			var tuple = Conversions.convert(input, typeof (decimal));

			Assert.IsTrue(tuple.Item1);
			Assert.AreEqual(Convert.ToDecimal(expected), tuple.Item2);
		}

		[TestCase("1,0%", 0.01)]
		[TestCase("1,5%", 0.015)]
		[TestCase("1%", 0.01)]
		[TestCase("100%", 1)]
		public void Convert_should_transform_input_into_percentage_obeying_the_thread_ci(string input, double expected)
		{
			var tuple = Conversions.convert(input, typeof (decimal));

			Assert.IsTrue(tuple.Item1);
			Assert.AreEqual(Convert.ToDecimal(expected), tuple.Item2);
		}

		[TestCase("true", true)]
		[TestCase("false", false)]
		public void Convert_should_transform_input_to_bool_based_on_the_current_ci(string input, bool expected)
		{
			var tuple = Conversions.convert(input, typeof (bool));

			Assert.IsTrue(tuple.Item1);
			Assert.AreEqual(expected, tuple.Item2);
		}

		[Test]
		public void Convert_should_transform_input_to_guid()
		{
			var guid = Guid.Parse("4D97BAB0-FC48-46CA-AA8C-77D209DCD9FF");

			var tuple = Conversions.convert("4D97BAB0-FC48-46CA-AA8C-77D209DCD9FF", typeof (Guid));

			Assert.IsTrue(tuple.Item1);
			Assert.AreEqual(guid, tuple.Item2);
		}

		[Test]
		public void Convert_should_transform_to_default_values_when_ICovertibles_with_empty_input()
		{
			Assert.AreEqual(null, Conversions.convert("", typeof (string)).Item2);
			Assert.AreEqual(0, Conversions.convert("", typeof (int)).Item2);
		}

		[Test]
		public void Convert_should_perform_simple_conversions_obeying_current_thread_ci()
		{
			Assert.AreEqual(1, Conversions.convert("1", typeof (int)).Item2);
			Assert.AreEqual(1.0, Conversions.convert("1,0", typeof (double)).Item2);
			Assert.AreEqual("asd", Conversions.convert("asd", typeof (string)).Item2);
		}

		[Test]
		public void Convert_should_transform_to_null_values_when_nullables_with_empty_input()
		{
			Assert.AreEqual(null, Conversions.convert("", typeof (int?)).Item2);
			Assert.AreEqual(null, Conversions.convert("", typeof (decimal?)).Item2);
		}

		[Test]
		public void Convert_should_transform_input_to_enum()
		{
			var tuple = Conversions.convert("Friday", typeof (DayOfWeek));

			Assert.IsTrue(tuple.Item1);
			Assert.AreEqual(DayOfWeek.Friday, tuple.Item2);
		}
	}
}
