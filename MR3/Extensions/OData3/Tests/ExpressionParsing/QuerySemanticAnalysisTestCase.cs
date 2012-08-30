//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using Microsoft.Data.Edm;
	using MonoRail.OData.Internal;
	using MonoRail.Tests;
	using NUnit.Framework;
	using OData3.Tests;

    [TestFixture]
	public class QuerySemanticAnalysisTestCase
	{
		[SetUp]
		public void Init()
		{
            _model = new Models.ModelWithAssociation();
            _model.InitializeModels(new StubServiceRegistry());
		}

		private QueryAst AnalyzeAndConvert(string expression, IEdmType rt)
		{
			var exp = QueryExpressionParser.parse_filter(expression);
			var tree = QuerySemanticAnalysis.analyze_and_convert(exp, rt);
			return tree;
		}

		private readonly string[] numericOps	= new[] { "sub", "add", "mul", "div", "mod" };
		// private readonly string[] relationalOps = new[] { "lt", "gt", "le", "ge" };
		private readonly string[][] numericTypes = new []
		                                           	{
		                                           		new [] { "Edm.Int32", "2" },
														new [] { "Edm.Int64", "3L" },
														new [] { "Edm.Decimal", "4m" },
														new [] { "Edm.Double", "5.0D" },
														new [] { "Edm.Single", "6F" },
		                                           	};

        private Models.ModelWithAssociation _model;

        [Test]
		public void NumericOps_On_NumericTypes()
		{
			foreach (var op in numericOps)
			foreach (var left in numericTypes)
			foreach (var right in numericTypes)
			{
				var exp = left[1] + " " + op + " " + right[1];
                var tree = AnalyzeAndConvert(exp, _model.EdmModel.FindType("schemaNs.Product"));
				Console.WriteLine("\t{0}\r\n{1}", exp, tree.ToStringTree());
			}
		}
	}
}
