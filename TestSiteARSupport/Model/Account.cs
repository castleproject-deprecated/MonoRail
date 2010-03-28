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
	using System;
	using System.Collections.Generic;
	using Castle.ActiveRecord;
	using Castle.Components.Validator;
	using Iesi.Collections.Generic;
	using ValidateEmailAttribute=Castle.Components.Validator.ValidateEmailAttribute;

	[ActiveRecord("TSAS_Account")]
	public class Account : ActiveRecordValidationBase
	{
		private String name;
		private IList<User> users = new List<User>();

		public Account()
		{
		}

		public Account(string name, string email, string password)
		{
			this.name = name;
			this.Email = email;
			this.Password = password;
			this.ConfirmationPassword = password;
		}

		[PrimaryKey]
		public int Id { get; set; }

		[Property, ValidateNonEmpty]
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		[Property]
		[ValidateNonEmpty]
		[ValidateEmail]
		public string Email { get; set; }

		[Property]
		[ValidateNonEmpty]
		public string Password { get; set; }

		[ValidateSameAs("Password")]
		public string ConfirmationPassword { get; set; }

		[BelongsTo("license_id")]
		public ProductLicense ProductLicense { get; set; }

		[HasAndBelongsToMany(Table = "AccountAccountPermission", ColumnRef = "permission_id", ColumnKey = "account_id", Inverse = false)]
		public ISet<AccountPermission> Permissions { get; set; }

		[HasMany]
		public IList<User> Users
		{
			get { return users; }
			set { users = value; }
		}

		public override string ToString()
		{
			return name;
		}

		public static Account[] FindAll()
		{
			return (Account[]) FindAll(typeof(Account));
		}
	}
}
