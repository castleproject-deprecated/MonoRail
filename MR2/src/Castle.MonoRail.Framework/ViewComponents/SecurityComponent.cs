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

using System;
using System.Linq;

namespace Castle.MonoRail.Framework.ViewComponents
{
	/// <summary>
	/// Only renders the body if the current user has the specified role(s)
	/// <example>
	/// <code>
	/// #blockcomponent(SecurityComponent with "roles=Manager,Admin")
	/// #authorized
	///		Content only available to admin or managers
	/// #end
	/// #notauthorized
	///		Content available to non admin or non managers
	/// #end
	/// #end
	/// </code>
	/// </example>
	/// </summary>
	[ViewComponentDetails("Security", Sections = "authorized,notauthorized")]
	public class SecurityComponent : ViewComponent
	{
		private bool shouldRender;

		/// <summary>
		/// Called by the framework once the component instance
		/// is initialized
		/// </summary>
		public override void Initialize()
		{
			string roles = (string)(ComponentParams["role"] ?? ComponentParams["roles"]);

			if (roles == null)
			{
				throw new MonoRailException("SecurityComponent: you must supply a roles parameter");
			}

			shouldRender = IsInRoles(roles);
		}

		/// <summary>
		/// Verify if the user is at least in one of the given role(s).
		/// </summary>
		/// <param name="roles">string (comma separated) representing an array of roles.</param>
		/// <returns><c>true</c> if the user is at least in one of the roles, otherwise <c>false</c>.</returns>
		protected virtual bool IsInRoles(string roles)
 		{
 			if (EngineContext.CurrentUser != null)
 			{
 				return roles.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).Any(itRole => EngineContext.CurrentUser.IsInRole(itRole.Trim()));
 			}
 			return false;
 		}

		/// <summary>
		/// Called by the framework so the component can
		/// render its content
		/// </summary>
		public override void Render()
		{
			if (shouldRender)
			{
				if (Context.HasSection("authorized"))
				{
					Context.RenderSection("authorized");
				}
				else
				{
					Context.RenderBody();
				}
			}
			else
			{
				if (Context.HasSection("notauthorized"))
				{
					Context.RenderSection("notauthorized");
				}
			}
		}
	}
}
