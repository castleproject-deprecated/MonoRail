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

using Castle.MonoRail.Framework.Internal;

namespace Castle.MonoRail.Framework.Services
{
	using Core;

	/// <summary>
	/// 
	/// </summary>
	public class DefaultServiceInitializer : IServiceInitializer
	{
		/// <summary>
		/// Initializes the specified service instance.
		/// </summary>
		/// <param name="serviceInstance">The service instance.</param>
		/// <param name="engineContext">The engine context.</param>
		public void Initialize(object serviceInstance, IEngineContext engineContext)
		{
			var controller = engineContext.CurrentController;
			var controllerCtx = engineContext.CurrentControllerContext;

			var ctxAware = serviceInstance as IContextAware;

			if (ctxAware != null)
			{
				ctxAware.SetContext(engineContext);
			}

			var aware = serviceInstance as IControllerAware;

			if (aware != null)
			{
				aware.SetController(controller, controllerCtx);
			}

			Initialize(serviceInstance, engineContext.Services);
		}

		/// <summary>
		/// Initializes the specified service instance.
		/// </summary>
		/// <param name="serviceInstance">The service instance.</param>
		/// <param name="container">The container.</param>
		public void Initialize(object serviceInstance, IMonoRailServices container)
		{
			var serviceEnabled = serviceInstance as IServiceEnabledComponent;

			if (serviceEnabled != null)
			{
				serviceEnabled.Service(container);
			}

			var mrServiceEnabled = serviceInstance as IMRServiceEnabled;

			if (mrServiceEnabled != null)
			{
				mrServiceEnabled.Service(container);
			}

			var initializable = serviceInstance as IInitializable;

			if (initializable != null)
			{
				initializable.Initialize();
			}
		}
	}
}
