namespace Castle.MonoRail.Hosting.Mvc.Typed
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Web;
	using System.Web.Routing;
	using Internal;
	using Primitives;

	[Export(typeof(ControllerProvider))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class ReflectionBasedControllerProvider : ControllerProvider
	{
		private readonly List<Tuple<string, Type>> validTypes;

		[ImportingConstructor]
		public ReflectionBasedControllerProvider(IHostingBridge source)
		{
			var assemblies = source.ReferencedAssemblies;

			validTypes = new List<Tuple<string, Type>>(
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

			var controllerType = validTypes
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
