// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.AspView.Internal
{
	using System;
	using System.Collections;
	
	///	<summary>
	///	Holding the properties of a view, differentiating the scope of the parent view from the current view's props
	///	</summary>
	public class ViewPropertiesDictionary : IDictionary
	{
		readonly IDictionary _parentScope;
		
		/// <summary>
		/// The local scope (not readonly) of the current view properties
		/// </summary>
		public IDictionary LocalScope { get; private set; }


		/// <summary>
		/// New <see cref="ViewPropertiesDictionary"/>
		/// </summary>
		/// <param name="parentScope">A dictionary representing the parent's scope</param>
		public ViewPropertiesDictionary(IDictionary parentScope)
		{
			_parentScope = parentScope;
			LocalScope = new ParametersDictionary();
		}

		/// <summary>
		/// New <see cref="ViewPropertiesDictionary"/> without a parent scope
		/// </summary>
		public ViewPropertiesDictionary() : this(null)
		{
		}

		public bool Contains(object key)
		{
			if (LocalScope.Contains(key)) return true;

			if (_parentScope != null)
				return _parentScope.Contains(key);
			
			return false;
		}

		/// <summary>
		/// Will add a new key/value to the local scope
		/// </summary>
		public void Add(object key, object value)
		{
			LocalScope.Add(key, value);
		}

		/// <summary>
		/// Will only clear the local scope
		/// </summary>
		public void Clear()
		{
			LocalScope.Clear();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return new ViewPropertiesDictionaryEnumerator(_parentScope, LocalScope);
		}

		/// <summary>
		/// Will remove the item with key=<paramref name="key"/> from the local scope
		/// </summary>
		public void Remove(object key)
		{
			LocalScope.Remove(key);
		}

		public object this[object key]
		{
			get { return LocalScope[key] ?? (_parentScope != null ? _parentScope[key] : null); }
			set { LocalScope[key] = value; }
		}

		public ICollection Keys
		{
			get { throw new NotImplementedException(); }
		}

		public ICollection Values
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return _parentScope.Count+LocalScope.Count; }
		}
		public object SyncRoot
		{
			get { return _parentScope.SyncRoot; }
		}

		public bool IsSynchronized
		{
			get { return false;}
		}

		class ViewPropertiesDictionaryEnumerator : IDictionaryEnumerator
		{
			readonly IDictionaryEnumerator _parentScope;
			readonly IDictionaryEnumerator _localScope;
			private IDictionaryEnumerator _currentScope;
			bool _inParentScope = true;

			public ViewPropertiesDictionaryEnumerator(IDictionary parentScope, IDictionary localScope)
			{
				_localScope = localScope.GetEnumerator();
				_parentScope = parentScope == null 
					? new EmptyDictionaryEnumerator() 
					: parentScope.GetEnumerator();

				_currentScope = _parentScope;
			}

			public bool MoveNext()
			{
				var currentScopeMoveNext = _currentScope.MoveNext();
				if (_inParentScope == false)
					return currentScopeMoveNext;

				if (currentScopeMoveNext)
					return true;

				_inParentScope = false;
				_currentScope = _localScope;
				return _currentScope.MoveNext();
			}

			public void Reset()
			{
				_parentScope.Reset();
				_localScope.Reset();
				_currentScope = _parentScope;
			}

			public object Current
			{
				get
				{
					if (_inParentScope)
						return _parentScope.Current;
					return _localScope.Current;
				}
			}

			public object Key
			{
				get
				{
					if (_inParentScope)
						return _parentScope.Key;
					return _localScope.Key;
				}
			}

			public object Value
			{
				get
				{
					if (_inParentScope)
						return _parentScope.Value;
					return _localScope.Value;
				}
			}

			public DictionaryEntry Entry
			{
				get
				{
					if (_inParentScope)
						return _parentScope.Entry;
					return _localScope.Entry;
				}
			}
		}

		class EmptyDictionaryEnumerator : IDictionaryEnumerator
		{
			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}

			public object Current
			{
				get { throw new NotImplementedException(); }
			}

			public object Key
			{
				get { throw new NotImplementedException(); }
			}

			public object Value
			{
				get { throw new NotImplementedException(); }
			}

			public DictionaryEntry Entry
			{
				get { throw new NotImplementedException(); }
			}
		}
	}


}