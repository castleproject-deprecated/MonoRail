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
namespace Castle.MonoRail.Internal
{
	using System;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.Threading;
	using System.Web;
	using Primitives;

	public static class MefExtensions
	{
		private static readonly object staticLocker = new object();
		private static bool hookedEndRequest = false;

		// Extension method to simplify filtering expression
		public static bool IsShared(this ComposablePartDefinition part)
		{
			object value;

			if (part.Metadata.TryGetValue(CompositionConstants.PartCreationPolicyMetadataName, out value))
			{
				return ((CreationPolicy) value) == CreationPolicy.Shared;
			}

			return false;
		}

		public static CompositionBatch Compose(this CompositionContainer container, IComposableHandler handler)
		{
			var batch = new CompositionBatch();

			batch.AddPart(handler);

			container.Compose(batch);

			return batch;
		}

		public static CompositionContainer HookOn(this CompositionContainer container, HttpContext httpContext)
		{
			httpContext.Items[ContainerManager.RequestContainerKey] = container;

			// Avoid race condition
			if (!hookedEndRequest)
			{
				lock (staticLocker)
				{
					if (!hookedEndRequest)
					{
						httpContext.ApplicationInstance.EndRequest += ContainerManager.OnEndRequestDisposeContainer;

						Thread.MemoryBarrier();
						hookedEndRequest = true;
					}
				}

			}

			return container;
		}

		public static CompositionContainer GetContainer(this HttpContextBase context)
		{
			return (CompositionContainer) context.Items[ContainerManager.RequestContainerKey];
		}

		public static string GetContract(this Type type)
		{
			return AttributedModelServices.GetContractName(type);
		}
	}
}
