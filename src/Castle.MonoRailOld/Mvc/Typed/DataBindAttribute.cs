#region License
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
#endregion
namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.Web;
	using Components.Binder;

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple=false)]
	public class DataBindAttribute : Attribute, IActionParameterBinder
	{
		public DataBindAttribute()
		{
		}

		public DataBindAttribute(string prefix)
		{
			Prefix = prefix;
		}

		public string Allow { get; set; }

		public string Exclude { get; set; }

		public string Prefix { get; set; }

		public object Bind(HttpContextBase httpContext, ParameterDescriptor descriptor)
		{
			var binder = new DataBinder();

			var node = new TreeBuilder().BuildSourceNode(httpContext.Request.Params);

			return binder.BindObject(descriptor.Type, Prefix ?? descriptor.Name, Exclude, Allow, node);
		}
	}
}
