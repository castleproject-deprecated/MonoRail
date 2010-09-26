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

using Castle.MicroKernel.Registration;

namespace Castle.MonoRail.WindsorExtension
{
	using Castle.Core;
	using Castle.MicroKernel.Facilities;
	using Castle.MonoRail.Framework;
	using Castle.MonoRail.Framework.Services;
	using Castle.MonoRail.Framework.Services.Utils;

	/// <summary>
	/// Facility responsible for registering the controllers in
	/// the controllerTree.
	/// </summary>
	public class MonoRailFacility : AbstractFacility
	{
		private IControllerTree controllerTree;
		private IViewComponentRegistry componentRegistry;

		protected override void Init()
		{
			RegisterWindsorLocatorStrategyWithinMonoRail();

			Kernel.Register(Component.For<IControllerTree>().ImplementedBy<DefaultControllerTree>().Named("mr.controllertree"));
			Kernel.Register(Component.For<IWizardPageFactory>().ImplementedBy<DefaultWizardPageFactory>().Named("mr.wizardpagefactory"));
			Kernel.Register(Component.For<IViewComponentRegistry>().ImplementedBy<DefaultViewComponentRegistry>().Named("mr.viewcomponentregistry"));
			Kernel.Register(Component.For<IControllerFactory>().ImplementedBy<WindsorControllerFactory>().Named("mr.controllerfactory"));
			Kernel.Register(Component.For<IFilterFactory>().ImplementedBy<WindsorFilterFactory>().Named("mr.filterFactory"));
			Kernel.Register(Component.For<IViewComponentFactory>().ImplementedBy<WindsorViewComponentFactory>().Named("mr.viewcompfactory"));
			Kernel.Register(Component.For<IHelperFactory>().ImplementedBy<WindsorHelperFactory>().Named("mr.helperfactory"));
			Kernel.Register(Component.For<IDynamicActionProviderFactory>().ImplementedBy<WindsorDynamicActionProviderFactory>().Named("mr.dynamicactionproviderfactory"));

			controllerTree = Kernel.Resolve<IControllerTree>();
			componentRegistry = Kernel.Resolve<IViewComponentRegistry>();

			Kernel.ComponentModelCreated += OnComponentModelCreated;
		}

		private static void RegisterWindsorLocatorStrategyWithinMonoRail()
		{
			ServiceProviderLocator.Instance.AddLocatorStrategy(new WindsorAccessorStrategy());
		}

		private void OnComponentModelCreated(ComponentModel model)
		{
			var isController = typeof(IController).IsAssignableFrom(model.Implementation);
			var isViewComponent = typeof(ViewComponent).IsAssignableFrom(model.Implementation);

			if (!isController && !isViewComponent)
			{
				return;
			}

			// Ensure it's transient
			model.LifestyleType = LifestyleType.Transient;
			model.InspectionBehavior = PropertiesInspectionBehavior.DeclaredOnly;

			if (isController)
			{
				var descriptor = ControllerInspectionUtil.Inspect(model.Implementation);

				controllerTree.AddController(descriptor.Area, descriptor.Name, model.Implementation);
			}

			if (isViewComponent)
			{
				componentRegistry.AddViewComponent(model.Name, model.Implementation);
			}
		}

		public class WindsorAccessorStrategy : ServiceProviderLocator.IAccessorStrategy
		{
			public IServiceProviderEx LocateProvider()
			{
				return WindsorContainerAccessorUtil.ObtainContainer();
			}
		}
	}
}
