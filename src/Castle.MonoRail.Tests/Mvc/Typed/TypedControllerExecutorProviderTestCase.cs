//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
namespace Castle.MonoRail.Tests.Mvc.Typed
{
	using System;
	using System.ComponentModel.Composition;
	using System.Web.Routing;
	using MonoRail.Mvc.Typed;
	using NUnit.Framework;

	[TestFixture]
	public class TypedControllerExecutorProviderTestCase
	{
		[Test]
		public void CreateExecutor_should_return_a_TypedControllerExecutor_ready_to_use()
		{
			var provider = new TypedControllerExecutorProvider {ExecutorFactory = new ExportFactory<TypedControllerExecutor>(GetExecutor)};
			var data = new RouteData();
			var meta = new TypedControllerMeta(null, null);

			var executor = (TypedControllerExecutor) provider.CreateExecutor(meta, data, null);

			Assert.AreSame(data, executor.RouteData);
			Assert.AreSame(meta, executor.Meta);
		}

		private static Tuple<TypedControllerExecutor, Action> GetExecutor()
		{
			return new Tuple<TypedControllerExecutor, Action>(new TypedControllerExecutor(null, null, null, null, null), () => {});
		}
	}
}
