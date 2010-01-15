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

namespace Castle.MonoRail.ActiveRecordSupport.Tests.ARDataBinderTests
{
	using System;
	using Framework.Test;
	using NUnit.Framework;
	using TestSiteARSupport.Model;

	[TestFixture]
	public class MR536 : BaseARTestCase
	{
		[Test]
		public void CanBindPrimaryKeyToEmptyGuid()
		{
			Tag tag = new Tag() { Id = Guid.Empty, Name = "TopMovie" };
			tag.Create();

			var request = new StubRequest();
			request.Params["tag.id"] = Guid.Empty.ToString();

			var binder = new ARDataBinder { AutoLoad = AutoLoadBehavior.Always, TreatEmptyGuidAsNull = false};
			
			var record = (Tag)binder.BindObject(typeof(Tag), "tag", request.ParamsNode);
	
			Assert.AreEqual(tag.Id, record.Id);
		}
	}
}