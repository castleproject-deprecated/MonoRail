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

namespace TestSiteARSupport.Model
{
	using Castle.ActiveRecord;
	
	[ActiveRecord("TSAS_PersonBase"), JoinedBase]
	public abstract class PersonBase : ActiveRecordBase
	{
		[PrimaryKey]
		public int Id { get; set; }

		[Property]
		public string First { get; set; }

		[Property]
		public string Middle { get; set; }

		[Property]
		public string Last { get; set; }
	}
}
