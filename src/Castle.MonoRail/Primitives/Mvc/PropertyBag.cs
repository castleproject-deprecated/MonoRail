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
namespace Castle.MonoRail.Primitives.Mvc
{
	using System.Collections.Generic;
	using System.Dynamic;

	public class PropertyBag : DynamicObject
	{
		private readonly Dictionary<string, object> _data = new Dictionary<string,object>();

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
            return _data.TryGetValue(binder.Name, out result);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
            _data[binder.Name] = value;

			return true;
		}
	}
}
