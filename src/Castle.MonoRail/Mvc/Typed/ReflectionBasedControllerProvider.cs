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
namespace Castle.MonoRail.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web.Routing;
	using Hosting.Internal;
	using Primitives.Mvc;

    [Export(typeof(ControllerProvider))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class ReflectionBasedControllerProvider : ControllerProvider
	{
		private readonly List<Tuple<string, Type>> _validTypes;

		[ImportingConstructor]
		public ReflectionBasedControllerProvider(IHostingBridge source)
		{
			var assemblies = source.ReferencedAssemblies;

            // very naive impl
			_validTypes = new List<Tuple<string, Type>>(
				assemblies
					.SelectMany(a => a.GetTypes())
					.Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract)
					.Select(t => new Tuple<string, Type>(t.Name.Substring(0, t.Name.Length - "Controller".Length).ToLowerInvariant(), t)));
		}

		[Import]
		public ControllerDescriptorBuilder DescriptorBuilder { get; set; }

		// No side effects
		public override ControllerMeta Create(RouteData data)
		{
			var controllerName = (string) data.Values["controller"];

			if (controllerName == null)
				return null;

			var controllerType = _validTypes
				.Where(t => string.CompareOrdinal(t.Item1, controllerName) == 0)
				.Select(t => t.Item2)
				.FirstOrDefault();

			if (controllerType == null)
				return null;

			var descriptor = DescriptorBuilder.Build(controllerType);

			var controller = Activator.CreateInstance(controllerType);
			var meta = new TypedControllerMeta(controller, descriptor);

			return meta;
		}
	}
}
