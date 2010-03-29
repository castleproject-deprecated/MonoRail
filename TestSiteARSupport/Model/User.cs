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
	using Castle.Components.Validator;
	using NHibernate.Criterion;

	[ActiveRecord("TSAS_User")]
	public class User
	{
		public User()
		{
		}

		public User(string name)
		{
			this.Name = name;
		}

		[PrimaryKey]
		public int Id { get; set; }

		[Property(NotNull = true)]
		[ValidateNonEmpty]
		public string Name { get; set; }

		[BelongsTo("account_id")]
		public Account Account { get; set; }

		public static User[] FindAll()
		{
			return ActiveRecordMediator<User>.FindAll(new[] { Order.Asc("Name") });
		}
	}
}
