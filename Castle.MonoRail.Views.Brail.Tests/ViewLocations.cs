namespace Castle.MonoRail.Views.Brail.Tests
{
	using System.IO;

	public class ViewLocations
	{
		public static string TestSiteBrail
		{
			get
			{
				if (Directory.Exists("../../../TestSiteBrail/Views"))
					return "../../../TestSiteBrail";
				
				if (Directory.Exists("../../../MonoRail/TestSiteBrail"))
					return "../../../MonoRail/TestSiteBrail";

				return "../../../src/TestSiteBrail";
			}
		}

		public static string BrailTestsView
		{
			get
			{
				if (Directory.Exists("../../../Castle.MonoRail.Views.Brail.Tests/Views"))
					return "../../../Castle.MonoRail.Views.Brail.Tests";

				if (Directory.Exists("../../../MonoRail/Castle.MonoRail.Views.Brail.Tests"))
					return "../../../MonoRail/Castle.MonoRail.Views.Brail.Tests";

				return "../../../src/Castle.MonoRail.Views.Brail.Tests";
			}
		}
	}
}