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
// 
namespace NVelocity.Test.Provider
{
	using System;

	/// <summary>
	/// simple class to test boolean property
	/// introspection - can't use TestProvider
	/// as there is a get( String )
	/// and that comes before isProperty
	/// in the search pattern
	/// </summary>
	/// <author> <a href="mailto:geirm@apache.org">Geir Magnusson Jr.</a></author>
	public class BoolObj
	{
		public Boolean isBoolean
		{
			get { return true; }
		}

		/*
	*  not isProperty as it's not
	*  boolean return valued...
	*/

		public String isNotboolean
		{
			get { return "hello"; }
		}
	}
}