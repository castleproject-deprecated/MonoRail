namespace Castle.MonoRail.Hosting.Mvc.Typed
{
	using System;
	using System.ComponentModel.Composition;
	using System.Diagnostics.Contracts;
	using System.Reflection;
	using Castle.MonoRail.Primitives.Mvc;

	[Export]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class ControllerDescriptorBuilder
	{
		//TODO: needs caching (per instance)
		public ControllerDescriptor Build(Type controllerType)
		{
			string name = controllerType.Name;
			
			name = name.Substring(0, name.Length - "Controller".Length).ToLowerInvariant();

			var controllerDesc = new ControllerDescriptor(controllerType, name, string.Empty);

			foreach (var method in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				var action = BuildActionDescriptor(method);

				if (action != null)
					controllerDesc.Actions.Add(action);
			}

			return controllerDesc;
		}

		private static ActionDescriptor BuildActionDescriptor(MethodInfo method)
		{
			if (method.IsSpecialName ||
				method.IsGenericMethodDefinition || method.IsStatic ||
				method.DeclaringType == typeof(object))
			{
				return null;
			}

			return new MethodInfoActionDescriptor(method);
		}
	}
}