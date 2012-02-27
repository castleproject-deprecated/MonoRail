namespace Castle.MonoRail.Integration.Tests
{
	using System;
	using System.IO;
	using CassiniDev;
	using NUnit.Framework;

	public abstract class BaseServerTest
	{
		private Server _server;

		public Server Server
		{
			get { return _server; }
		}

		[TestFixtureSetUp]
		public void Init()
		{
			var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while (true)
			{
				if (!dir.Name.Equals("tests", StringComparison.OrdinalIgnoreCase))
				{
					dir = dir.Parent;
				}
				else break;
			}

			_server = new Server(1302, "/", Path.Combine(dir.FullName, "WebSiteForIntegration"), false, true);
			_server.Start();
		}

		[TestFixtureTearDown]
		public void Terminate()
		{
			if (_server != null) _server.Dispose();
		}
	}
}
