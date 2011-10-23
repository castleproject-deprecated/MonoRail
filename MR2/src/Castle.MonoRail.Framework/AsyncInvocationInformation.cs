// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework
{
	using System;

	/// <summary>
	/// Information about the async invocation of the current action
	/// </summary>
	public class AsyncInvocationInformation
	{
		/// <summary>
		/// Gets or sets the state to be passed to the async controller
		/// </summary>
		/// <value>The state.</value>
		public object State { get; set; }

		/// <summary>
		/// Gets or sets the async result.
		/// </summary>
		/// <value>The async result.</value>
		public IAsyncResult Result { get; set; }

		/// <summary>
		/// Gets or sets the async callback.
		/// </summary>
		/// <value>The async callback.</value>
		public AsyncCallback Callback { get; set; }
	}
}