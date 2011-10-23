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
namespace NVelocity.Test
{
	using System;
	using System.Collections;
	using System.Text;

	public class AjaxHelper2
	{
		public String LinkToRemote(String name, String url, IDictionary options)
		{
			if (options == null) throw new ArgumentNullException("options");

			StringBuilder sb = new StringBuilder(name + " " + url + " ");


			Array keysSorted = (new ArrayList(options.Keys)).ToArray(typeof(string)) as string[];

			Array.Sort(keysSorted);

			foreach(string key in keysSorted)
			{
				sb.Append(key).Append("=<").Append(options[key]).Append("> ");
			}

			sb.Length--;

			return sb.ToString();
		}

		public String LinkToRemote(String name, String url, string options)
		{
			if (options == null) throw new ArgumentNullException("options");

			return name + " " + url + " " + options;
		}
	}
}