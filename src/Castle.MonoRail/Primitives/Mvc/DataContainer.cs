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
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Runtime.Serialization;

	public class DataContainer : DynamicObject, IDictionary<string, object>
	{
		private readonly Dictionary<string, object> _inner = new Dictionary<string, object>();

		public object MainModel { get; set; }

		#region IDictionary<string,object>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _inner).GetEnumerator();
		}

		public void CopyTo(Array array, int index)
		{
			((ICollection) _inner).CopyTo(array, index);
		}

		public object SyncRoot
		{
			get { return _inner; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public bool Contains(object key)
		{
			return ((IDictionary) _inner).Contains(key);
		}

		public void Add(object key, object value)
		{
			((IDictionary) _inner).Add(key, value);
		}

		public void Remove(object key)
		{
			((IDictionary) _inner).Remove(key);
		}

		public object this[object key]
		{
			get { return _inner[key.ToString()]; }
			set { _inner[key.ToString()] = value; }
		}

		public void Add(string key, object value)
		{
			_inner.Add(key, value);
		}

		public void Clear()
		{
			_inner.Clear();
		}

		public bool ContainsKey(string key)
		{
			return _inner.ContainsKey(key);
		}

		public bool ContainsValue(object value)
		{
			return _inner.ContainsValue(value);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			_inner.GetObjectData(info, context);
		}

		public void OnDeserialization(object sender)
		{
			_inner.OnDeserialization(sender);
		}

		public bool Remove(string key)
		{
			return _inner.Remove(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _inner.TryGetValue(key, out value);
		}

		public IEqualityComparer<string> Comparer
		{
			get { return _inner.Comparer; }
		}

		public int Count
		{
			get { return _inner.Count; }
		}

		public object this[string key]
		{
			get { return _inner[key]; }
			set { _inner[key] = value; }
		}

		bool ICollection<KeyValuePair<String, object>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<String, object>>.Contains(KeyValuePair<String, object> item)
		{
			return _inner.ContainsKey(item.Key);
		}

		public ICollection<string> Keys
		{
			get { return _inner.Keys; }
		}

		void ICollection<KeyValuePair<String, object>>.CopyTo(KeyValuePair<String, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<String, object> item)
		{
			return _inner.Remove(item.Key);
		}

		void ICollection<KeyValuePair<String, object>>.Add(KeyValuePair<String, object> item)
		{
			_inner.Add(item.Key, item.Value);
		}

		public ICollection<object> Values
		{
			get { return _inner.Values; }
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _inner.GetEnumerator();
		}

		#endregion

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!_inner.ContainsKey(binder.Name))
				result = null;
			else
				result = _inner[binder.Name];

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_inner[binder.Name] = value;

			return true;
		}
	}
}
