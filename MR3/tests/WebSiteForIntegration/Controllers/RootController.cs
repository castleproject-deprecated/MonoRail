namespace WebSiteForIntegration.Controllers
{
	using System;
	using System.IO;
	using Castle.MonoRail;

	public class RootController
	{
		public ActionResult Index()
		{
			return new OutputWriterResult(WriteMsg);
		}

		private void WriteMsg(Stream stream)
		{
			var writer = new StreamWriter(stream);
			writer.Write("Howdy");
			writer.Flush();
		}
	}
}