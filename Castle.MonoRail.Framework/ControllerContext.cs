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

namespace Castle.MonoRail.Framework
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using Descriptors;
	using Resources;
	using Routing;

	/// <summary>
	/// Pendent
	/// </summary>
	public class ControllerContext : IControllerContext
	{
		private IDictionary<string, object> customActionParameters =
			new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		private IDictionary propertyBag = new HybridDictionary(true);
		private HelperDictionary helpers = new HelperDictionary();

		private readonly IDictionary<string, IDynamicAction> dynamicActions =
			new Dictionary<string, IDynamicAction>(StringComparer.InvariantCultureIgnoreCase);

		private readonly IDictionary<string, IResource> resources =
			new Dictionary<string, IResource>(StringComparer.InvariantCultureIgnoreCase);

		private AsyncInvocationInformation asyncInformation = new AsyncInvocationInformation();

		/// <summary>
		/// Initializes a new instance of the <see cref="ControllerContext"/> class.
		/// </summary>
		public ControllerContext()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ControllerContext"/> class.
		/// </summary>
		/// <param name="name">The controller name.</param>
		/// <param name="action">The action name.</param>
		/// <param name="metaDescriptor">The meta descriptor.</param>
		public ControllerContext(string name, string action, ControllerMetaDescriptor metaDescriptor) :
			this(name, string.Empty, action, metaDescriptor)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ControllerContext"/> class.
		/// </summary>
		/// <param name="name">The controller name.</param>
		/// <param name="areaName">The area name.</param>
		/// <param name="action">The action name.</param>
		/// <param name="metaDescriptor">The meta descriptor.</param>
		public ControllerContext(string name, string areaName, string action, ControllerMetaDescriptor metaDescriptor)
		{
			this.Name = name;
			this.AreaName = areaName;
			this.Action = action;
			this.ControllerDescriptor = metaDescriptor;
		}

		/// <summary>
		/// Gets or sets the custom action parameters.
		/// </summary>
		/// <value>The custom action parameters.</value>
		public IDictionary<string, object> CustomActionParameters
		{
			get { return customActionParameters; }
			set { customActionParameters = value; }
		}

		/// <summary>
		/// Gets the property bag, which is used
		/// to pass variables to the view.
		/// </summary>
		/// <value></value>
		public IDictionary PropertyBag
		{
			get { return propertyBag; }
			set { propertyBag = value; }
		}

		/// <summary>
		/// Gets a dictionary of name/helper instance
		/// </summary>
		/// <value>The helpers.</value>
		public HelperDictionary Helpers
		{
			get { return helpers; }
			set { helpers = value; }
		}

		/// <summary>
		/// Gets the controller's name.
		/// </summary>
		/// <value></value>
		public string Name { get; set; }

		/// <summary>
		/// Gets the controller's area name.
		/// </summary>
		/// <value></value>
		public string AreaName { get; set; }

		/// <summary>
		/// Gets or set the layout being used.
		/// </summary>
		/// <value></value>
		public string[] LayoutNames { get; set; }

		/// <summary>
		/// Gets the name of the action being processed.
		/// </summary>
		/// <value></value>
		public string Action { get; set; }

		/// <summary>
		/// Gets or sets the view which will be rendered after this action executes.
		/// </summary>
		/// <value></value>
		public string SelectedViewName { get; set; }

		/// <summary>
		/// Gets the view folder -- (areaname +
		/// controllername) or just controller name -- that this controller
		/// will use by default.
		/// </summary>
		/// <value></value>
		public string ViewFolder { get; set; }

		/// <summary>
		/// Gets a dicitionary of name/<see cref="IResource"/>
		/// </summary>
		/// <value>The resources.</value>
		/// <remarks>It is supposed to be used by MonoRail infrastructure only</remarks>
		public IDictionary<string, IResource> Resources
		{
			get { return resources; }
		}

		/// <summary>
		/// Gets the dynamic actions.
		/// </summary>
		/// <value>The dynamic actions.</value>
		public IDictionary<string, IDynamicAction> DynamicActions
		{
			get { return dynamicActions; }
		}

		/// <summary>
		/// Gets or sets the controller descriptor.
		/// </summary>
		/// <value>The controller descriptor.</value>
		public ControllerMetaDescriptor ControllerDescriptor { get; set; }

		/// <summary>
		/// Gets or sets the route match.
		/// </summary>
		/// <value>The route match.</value>
		public RouteMatch RouteMatch { get; set; }

		/// <summary>
		/// Get or set the information used to manage async invocations
		/// </summary>
		public AsyncInvocationInformation Async
		{
			get { return asyncInformation; }
			set { asyncInformation = value; }
		}
	}
}