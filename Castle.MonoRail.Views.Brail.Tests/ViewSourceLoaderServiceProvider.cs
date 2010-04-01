namespace Castle.MonoRail.Views.Brail.Tests
{
	using System;
	using Framework;

	internal class ViewSourceLoaderServiceProvider : IServiceProvider
	{
		private string viewRootDir;

		public ViewSourceLoaderServiceProvider(string viewRootDir)
		{
			this.viewRootDir = viewRootDir;
		}

		#region IServiceProvider Members

		public object GetService(Type serviceType)
		{
			if (typeof (IViewSourceLoader) == serviceType)
			{
				return new FileAssemblyViewSourceLoader
				{
					ViewRootDir = viewRootDir
				};
			}
			return null;
		}

		#endregion
	}
}