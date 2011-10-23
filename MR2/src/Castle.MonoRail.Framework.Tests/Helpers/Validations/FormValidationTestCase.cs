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

namespace Castle.MonoRail.Framework.Tests.Helpers.Validations
{
	using System;
	using System.Globalization;
	using System.Threading;
	using Components.Validator;
	using Framework.Helpers;
	using Controllers;
	using NUnit.Framework;

	[TestFixture]
	public class FormValidationTestCase
	{
		private FormHelper helper;
		private ModelWithValidation model;

		[SetUp]
		public void Init()
		{
			var en = CultureInfo.CreateSpecificCulture("en");

			Thread.CurrentThread.CurrentCulture	= en;
			Thread.CurrentThread.CurrentUICulture = en;

			helper = new FormHelper();
			model = new ModelWithValidation();

			var controller = new HomeController();
			var controllerContext = new ControllerContext();

			controllerContext.PropertyBag.Add("model", model);

			helper.SetController(controller, controllerContext);
		}

		[Test]
		public void ValidationIsGeneratedForModel()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));

			Assert.AreEqual("<input type=\"text\" id=\"model_nonemptyfield\" " + 
				"name=\"model.nonemptyfield\" value=\"\" class=\"required\" " +
				"title=\"This is a required field.\" />", helper.TextField("model.nonemptyfield"));

			Assert.AreEqual("<input type=\"text\" id=\"model_emailfield\" " +
				"name=\"model.emailfield\" value=\"\" class=\"validate-email\" " +
				"title=\"Email doesnt look right.\" />", helper.TextField("model.emailfield"));

			// Attribute order cannot be guaranted, so this test may fail ocasionally
			// Assert.AreEqual("<input type=\"text\" id=\"model_nonemptyemailfield\" " +
			//	"name=\"model.nonemptyemailfield\" value=\"\" class=\"validate-email required\" " +
			//	"title=\"Please enter a valid email address. For example fred@domain.com, This is a required field\" />", helper.TextField("model.nonemptyemailfield"));

			helper.EndFormTag();
		}

		[Test]
		public void UsingScopes()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));
			helper.Push("model");

			Assert.AreEqual("<input type=\"text\" id=\"model_nonemptyfield\" " +
				"name=\"model.nonemptyfield\" value=\"\" class=\"required\" " +
				"title=\"This is a required field.\" />", helper.TextField("nonemptyfield"));

			Assert.AreEqual("<input type=\"text\" id=\"model_emailfield\" " +
				"name=\"model.emailfield\" value=\"\" class=\"validate-email\" " +
				"title=\"Email doesnt look right.\" />", helper.TextField("emailfield"));

			// Attribute order cannot be guaranted, so this test may fail ocasionally
			// Assert.AreEqual("<input type=\"text\" id=\"model_nonemptyemailfield\" " +
			//	"name=\"model.nonemptyemailfield\" value=\"\" class=\"validate-email required\" " +
			//	"title=\"Please enter a valid email address. For example fred@domain.com, This is a required field\" />", helper.TextField("nonemptyemailfield"));

			helper.Pop();
			helper.EndFormTag();
		}

		[Test]
		public void ValidationForSelects()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));

			Assert.AreEqual("<select id=\"model_city\" " +
				"name=\"model.city\" class=\"validate-selection\" " +
				"title=\"This is a required field.\" >\r\n" + 
				"<option value=\"0\">---</option>\r\n" +
				"<option value=\"Sao Paulo\">Sao Paulo</option>\r\n" +
				"<option value=\"Sao Carlos\">Sao Carlos</option>\r\n" +
				"</select>", 
				helper.Select("model.city", 
					new[] { "Sao Paulo", "Sao Carlos" }, DictHelper.Create("firstoption=---")));

			helper.EndFormTag();
		}

		[Test]
		public void ValidationAreInherited()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));

			Assert.AreEqual("<select id=\"model_city_id\" " +
				"name=\"model.city.id\" class=\"validate-selection\" " +
				"title=\"This is a required field.\" >\r\n" +
				"<option value=\"0\">---</option>\r\n" +
				"<option value=\"1\">1</option>\r\n" +
				"<option value=\"2\">2</option>\r\n" +
				"</select>",
				helper.Select("model.city.id",
					new[] { "1", "2" }, DictHelper.Create("firstoption=---")));

			helper.EndFormTag();
		}

		[Test]
		public void ValidateSameAsIsPrefixedWithModelName()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));

			Assert.AreEqual("<input type=\"text\" id=\"model_ConfirmedEmailField\" " +
	"name=\"model.ConfirmedEmailField\" value=\"\" class=\"validate-same-as-model_EmailField\" " +
	"title=\"Fields do not match.\" />", helper.TextField("model.ConfirmedEmailField"));

			helper.EndFormTag();
		}
		[Test]
		public void ValidateNotSameAsIsPrefixedWithModelName()
		{
			helper.FormTag(DictHelper.Create("noaction=true"));

			Assert.AreEqual("<input type=\"text\" id=\"model_Name\" " +
	"name=\"model.Name\" value=\"\" class=\"validate-not-same-as-model_EmailField\" " +
	"title=\"Fields should not match.\" />", helper.TextField("model.Name"));

			helper.EndFormTag();
		}
	}

	public class ModelWithValidation
	{
		private string groupValue1;
		private string groupValue2;

		[ValidateNonEmpty]
		public Country Country { get; set; }

		[ValidateNonEmpty]
		public string City { get; set; }

		[ValidateNonEmpty]
		public string NonEmptyField { get; set; }

		[ValidateEmail("Email doesnt look right")]
		public string EmailField { get; set; }

		[ValidateNonEmpty]
		[ValidateEmail]
		public string NonEmptyEmailField { get; set; }

		[ValidateSameAs("EmailField")]
		public string ConfirmedEmailField { get; set; }

		[ValidateRegExp(@"[\w-]+@([\w-]+\.)+[\w-]+")]
		public string RegExEmailField { get; set; }

		[ValidateNotSameAs("EmailField")]
		public string Name { get; set; }

		[ValidateInteger]
		public int FirstValue { get; set; }

		[ValidateIsLesser(IsLesserValidationType.Integer, "FirstValue")]
		public int SecondValue { get; set; }

		[ValidateInteger]
		public int ThirdValue { get; set; }

		[ValidateIsGreater(IsGreaterValidationType.Integer, "ThirdValue")]
		public int ForthValue { get; set; }

		[ValidateNonEmpty]
		[ValidateLength(Int32.MinValue, 10)]
		public string MaxLength { get; set; }

		[ValidateNonEmpty]
		[ValidateLength(10, Int32.MaxValue)]
		public string MinLength { get; set; }

		[ValidateGroupNotEmpty("mygroup1")]
		public string GroupValue1
		{
			get { return groupValue1; }
			set { groupValue1 = value; }
		}

		[ValidateGroupNotEmpty("mygroup1")]
		public string GroupValue2
		{
			get { return groupValue2; }
			set { groupValue2 = value; }
		}

		[ValidateGroupNotEmpty("mygroup2")]
		public string GroupValue3
		{
			get { return groupValue1; }
			set { groupValue1 = value; }
		}

		[ValidateGroupNotEmpty("mygroup2")]
		public string GroupValue4
		{
			get { return groupValue2; }
			set { groupValue2 = value; }
		}
	}

	public class Country
	{
		public int Id { get; set; }
	}
}
