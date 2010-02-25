namespace Castle.MonoRail.Views.Brail.Tests
{
	using System.Configuration;
	using System.IO;

	public class ViewLocations
	{
		private const string AppPathTests = "tests.src";
		private const string AppPathWeb = "web.physical.dir";

		public static string TestSiteBrail
		{
			get
			{
				string webAppPath = Path.Combine(ConfigurationManager.AppSettings[AppPathTests], ConfigurationManager.AppSettings[AppPathWeb]);
				if (Directory.Exists(Path.Combine(webAppPath, "Views")))
					return new DirectoryInfo(webAppPath).FullName;

				throw new ConfigurationErrorsException("Unable to find views on TestSiteBrail. Check the key " + AppPathTests + " and " + AppPathWeb + "in app.config/appSettings");
			}
		}

		public static string BrailTestsView
		{
			get
			{
				if (Directory.Exists(Path.Combine(ConfigurationManager.AppSettings[AppPathTests], "Views")))
					return new DirectoryInfo(ConfigurationManager.AppSettings[AppPathTests]).FullName;

				throw new ConfigurationErrorsException("Unable to find Brail test views. Check the key " + AppPathTests + " in app.config/appSettings");
			}
		}
	}
}